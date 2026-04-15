import path from "path";
import { app } from "electron";
import { PrismaClient } from "../../src/generated/prisma/client";

let prisma: PrismaClient | null = null;
let prismaUnavailableLogged = false;

export function getPrismaClient() {
  if (prisma) {
    return prisma;
  }

  try {
    const userDataPath = app.getPath("userData");
    process.env.DATABASE_URL = `file:${path.join(userDataPath, "maba-control-center.db")}`;
    const prismaOptions = {} as ConstructorParameters<typeof PrismaClient>[0];
    prisma = new PrismaClient(prismaOptions);
  } catch (error) {
    if (!prismaUnavailableLogged) {
      prismaUnavailableLogged = true;
      console.warn("Prisma client unavailable in current local setup; continuing without local DB.", error);
    }

    return null;
  }

  return prisma;
}

