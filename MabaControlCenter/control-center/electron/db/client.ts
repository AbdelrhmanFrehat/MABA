import path from "path";
import { app } from "electron";
import { PrismaClient } from "../../src/generated/prisma/client";

let prisma: PrismaClient | null = null;

export function getPrismaClient() {
  if (!prisma) {
    const userDataPath = app.getPath("userData");
    process.env.DATABASE_URL = `file:${path.join(userDataPath, "maba-control-center.db")}`;
    const prismaOptions = {} as ConstructorParameters<typeof PrismaClient>[0];
    prisma = new PrismaClient(prismaOptions);
  }
  return prisma;
}

