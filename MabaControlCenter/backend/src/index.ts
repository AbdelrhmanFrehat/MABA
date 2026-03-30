import Fastify from "fastify";

async function buildServer() {
  const app = Fastify({
    logger: {
      level: "info"
    }
  });

  app.get("/health", async () => {
    return { status: "ok" };
  });

  return app;
}

async function start() {
  const app = await buildServer();
  const port = Number(process.env.PORT || 4000);
  const host = process.env.HOST || "0.0.0.0";

  try {
    await app.listen({ port, host });
    app.log.info(`Backend listening on http://${host}:${port}`);
  } catch (err) {
    app.log.error(err);
    process.exit(1);
  }
}

start();

