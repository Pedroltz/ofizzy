using System.Formats.Asn1;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Ofizzy.Api.Infrastructure.Errors;

namespace Ofizzy.Api.Modules.Fiscal;

public sealed class FiscalCertificateVault(IConfiguration configuration)
{
    public string ActiveKeyId => configuration["Fiscal:ActiveKeyId"] ?? string.Empty;

    public bool Configured => !string.IsNullOrWhiteSpace(ActiveKeyId)
        && !string.IsNullOrWhiteSpace(configuration[$"Fiscal:Keys:{ActiveKeyId}"]);

    private byte[] Key(string id)
    {
        try
        {
            var key = Convert.FromBase64String(configuration[$"Fiscal:Keys:{id}"] ?? string.Empty);
            if (key.Length != 32)
            {
                throw new FormatException();
            }

            return key;
        }
        catch (FormatException)
        {
            throw new ConflictException("A chave de proteção fiscal não está configurada no servidor. Contate o administrador.");
        }
    }

    public byte[] Protect(Guid tenant, ReadOnlySpan<byte> data)
    {
        var key = Key(ActiveKeyId);
        try
        {
            var nonce = RandomNumberGenerator.GetBytes(12);
            var tag = new byte[16];
            var cipher = new byte[data.Length];

            using var aes = new AesGcm(key, 16);
            aes.Encrypt(
                nonce,
                data,
                cipher,
                tag,
                Encoding.UTF8.GetBytes($"ofizzy:fiscal:a1:{tenant}:{ActiveKeyId}"));

            return [.. nonce, .. tag, .. cipher];
        }
        finally
        {
            CryptographicOperations.ZeroMemory(key);
        }
    }

    public byte[] Unprotect(Guid tenant, string keyId, byte[] data)
    {
        var key = Key(keyId);
        try
        {
            if (data.Length < 28)
            {
                throw new CryptographicException();
            }

            var plain = new byte[data.Length - 28];
            using var aes = new AesGcm(key, 16);
            aes.Decrypt(
                data.AsSpan(0, 12),
                data.AsSpan(28),
                data.AsSpan(12, 16),
                plain,
                Encoding.UTF8.GetBytes($"ofizzy:fiscal:a1:{tenant}:{keyId}"));

            return plain;
        }
        finally
        {
            CryptographicOperations.ZeroMemory(key);
        }
    }

    public X509Certificate2 Load(FiscalSettings settings)
    {
        if (settings.Certificate is null || settings.KeyId is null)
        {
            throw new ConflictException("Cadastre o certificado A1 da empresa.");
        }

        byte[]? plain = null;
        try
        {
            plain = Unprotect(settings.TenantId, settings.KeyId, settings.Certificate);
            return X509CertificateLoader.LoadPkcs12(plain, null, X509KeyStorageFlags.EphemeralKeySet);
        }
        catch (CryptographicException)
        {
            throw new ConflictException("Não foi possível abrir o certificado. Verifique a chave de proteção e o certificado cadastrado.");
        }
        finally
        {
            if (plain != null)
            {
                CryptographicOperations.ZeroMemory(plain);
            }
        }
    }

    public static bool MatchesCnpj(X509Certificate2 certificate, string cnpj)
    {
        // ICP-Brasil CNPJ is the otherName 2.16.76.1.3.3 in Subject Alternative Name.
        var san = certificate.Extensions["2.5.29.17"];
        if (san is null)
        {
            return false;
        }

        try
        {
            var reader = new AsnReader(san.RawData, AsnEncodingRules.DER).ReadSequence();
            while (reader.HasData)
            {
                if (!reader.PeekTag().HasSameClassAndValue(new Asn1Tag(TagClass.ContextSpecific, 0)))
                {
                    reader.ReadEncodedValue();
                    continue;
                }

                var name = reader.ReadSequence(new Asn1Tag(TagClass.ContextSpecific, 0, true));
                var oid = name.ReadObjectIdentifier();
                var value = name.ReadSequence(new Asn1Tag(TagClass.ContextSpecific, 0, true));

                if (oid != "2.16.76.1.3.3")
                {
                    continue;
                }

                var tag = value.PeekTag();
                var text = tag.TagValue == 4
                    ? Encoding.ASCII.GetString(value.ReadOctetString())
                    : value.ReadCharacterString((UniversalTagNumber)tag.TagValue);

                return text == cnpj;
            }
        }
        catch (AsnContentException)
        {
            return false;
        }

        return false;
    }
}
