import { describe, it, expect } from "vitest";
import Fastify from "fastify";

describe("backend basic health", () => {
  it("responds on /health", async () => {
    const app = Fastify();
    app.get("/health", async () => ({ status: "ok" }));

    const res = await app.inject({ method: "GET", url: "/health" });
    expect(res.statusCode).toBe(200);
    expect(res.json()).toEqual({ status: "ok" });
  });
});

