# Segurança

Reporte vulnerabilidades de forma privada ao responsável pela instalação. Não abra issues públicas com credenciais ou dados de clientes.

As sessões usam cookies HttpOnly/SameSite, proteção CSRF e refresh token com hash e rotação. Produção exige HTTPS, chave JWT aleatória com no mínimo 32 bytes e senhas exclusivas. Logs não devem conter senha, token, documento ou payload completo de clientes.
