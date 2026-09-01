# ADR 0003 — Sessões do navegador

Status: aceito — 2026-08-31

Access JWT de 15 minutos e refresh token de 30 dias ficam em cookies HttpOnly/SameSite Strict. Mutations exigem antiforgery token. Refresh tokens são aleatórios, armazenados como hash e rotacionados; reutilização revoga a família.
