import { getPrismaClient } from "../db/client";

export class SyncEngine {
  private timer: NodeJS.Timeout | null = null;

  start(intervalMs = 60_000) {
    if (this.timer) return;
    this.timer = setInterval(() => {
      void this.syncOnce();
    }, intervalMs);
  }

  stop() {
    if (this.timer) {
      clearInterval(this.timer);
      this.timer = null;
    }
  }

  async syncOnce() {
    const prisma = getPrismaClient();
    // Placeholder: later call backend and reconcile entities.
    await prisma.syncState.findMany();
  }
}

