# SOK.NET
Complete rebuild of SOK system on .NET Core platform, using ASP.NET Core MVC.

## Prerequisites

To run the application, you need to have installed following software:
- [Docker](https://www.docker.com/get-started)
- [Docker Compose](https://docs.docker.com/compose/install/)
- [npm](https://www.npmjs.com/get-npm) (optionally, for CSS building in development)

The main tool to run the application is `docker compose`, which uses `docker-compose.yml` as the base file.
You can extend it with additional files for different environments, like development or production.

In our case we have two additional files:
- `docker-compose.prod.yml` for production environment,
- `docker-compose.vs-code.yml` for enhanced development environment (enabling hot-reload).

## Environment Variables

The application uses a `.env` file to load environment variables for Docker Compose. In the repository you can find a sample file named `.env.sample` with placeholder or default values. You should create your own `.env` file based on the sample and adjust the values as needed before running the app first time.

### Encryption Key

The application uses `CRYPTO_KEY` environment variable to encrypt sensitive data (parish database connection strings). 

**For new installations**, simply set any secure password:
```env
CRYPTO_KEY=MySecureParishPassword2026!
```

The system uses PBKDF2 to automatically derive a 256-bit encryption key from your password.

**For existing installations** with Base64-encoded keys, see [docs/CRYPTO_MIGRATION.md](docs/CRYPTO_MIGRATION.md) for migration guide.

For key rotation and advanced configuration, see [docs/KEY_ROTATION.md](docs/KEY_ROTATION.md).

## Development

To run a development environment simply use the following command:

```shell
docker compose up
```

You can then enter the site by going to:

```
http://localhost:8060
```

When you make changes to the code, you need to rebuild the application container to see the changes reflected in the running app:

```shell
docker compose up --build
```

### Development with Hot-Reload (VS Code)

To enable hot-reload functionality, you can use the `docker-compose.vs-code.yml` file as an override to the base compose file:

```shell
docker compose -f docker-compose.yml -f docker-compose.vs-code.yml up --build
```

This will mount the source code from your host machine into the application container, allowing changes to be reflected immediately without rebuilding the container.

Additionally you may want to enable Tailwind's CSS watcher to automatically recompile CSS on changes:

```shell
cd SOK.Web
npm install # first time only
npm run watch:css
```

## Production

To run the app on the production server just use the override compose file for production:

```shell
docker compose -f docker-compose.prod.yml up --build -d
```

The flags used are:
- `--build` to build the images before starting the containers,
- `-d` to run the containers in detached mode (in the background).

The command brings up three services:

- _app_ for the application container,
- _db_ for the database container,
- _traefik_ for reverse proxy and managing SSL/TLS certificates.

> [!IMPORTANT]
> Make sure to properly set all environment variables in the `.env` file before running in production mode (especially first time).
