# JWT RSA Keys (RS256)

The Core auth service signs JWT access tokens with **RS256** by default. It needs an RSA key pair:

- `jwt-private.pem` — used to **sign** tokens (keep secret, never commit)
- `jwt-public.pem` — used to **validate** tokens (safe to distribute to other services)

Paths are configured in `appsettings.json` under `Jwt:PrivateKeyPath` / `Jwt:PublicKeyPath`
(relative to the service working directory), or provided inline via `Jwt:PrivateKeyPem` /
`Jwt:PublicKeyPem`. In containers/production, inject these through environment variables or a
secret store (e.g. `Jwt__PrivateKeyPem`).

## Generate a development key pair

Using OpenSSL:

```powershell
# Private key (PKCS#8, PEM)
openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:2048 -out jwt-private.pem

# Public key (SPKI, PEM)
openssl rsa -in jwt-private.pem -pubout -out jwt-public.pem
```

Or using the .NET CLI (no OpenSSL required):

```powershell
dotnet script ../generate-jwt-keys.csx   # if you use dotnet-script
```

## Falling back to HS256

Set `Jwt:Algorithm` to `HS256` and provide a `Jwt:SigningKey` of at least 32 characters. In that
mode the RSA key files are ignored.

> The `keys/` directory is git-ignored for `*.pem` files so private keys are never committed.
