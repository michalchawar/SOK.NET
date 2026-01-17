# SOK.NET
Complete rebuild of SOK system on .NET Core platform, using ASP.NET Core MVC.

## Development

To run a development environment you need to start all services, using `docker compose` with extension files:

```shell
docker compose -f docker-compose.override.yml -f docker-compose.vs-code.yml up
```

You can then enter the site by going to:

```
http://localhost:8060
```

In the docker container runs a service, that watches the changes you are making in the code and hot-reloads them into the app, so you can just refresh the page after saving the file.

Moreover, if you want to also have Tailwind CSS stylesheet dynamically rebuilding, locally run:

```shell
cd SOK.Web
npm run watch:css
```

### MS Visual Studio

If you want to develop in Visual Studio you can use the features embedded in the software (skipping the `docker-compose.vs-code.yml`).

## Production

To run the app on the production server, set the content of `.env` file to your liking (at least at the first time), and just include the production compose override:

```shell
docker compose -f docker-compose.prod.yml up
```

It brings up three services:

- _app_ for the application container,
- _db_ for the database container,
- _traefik_ for reverse proxy and managing SSL/TLS certificates.
