import path from "path";
import { app } from "electron";
import { PrismaClient } from "../src/generated/prisma";

let prisma: PrismaClient | null = null;

export function getPrismaClient() {
  if (!prisma) {
    const userDataPath = app.getPath("userData");
    process.env.DATABASE_URL = `file:${path.join(userDataPath, "maba-control-center.db")}`;
    prisma = new PrismaClient();
  }
  return prisma;
}

