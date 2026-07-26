import { getJson, postJson } from "./http";

export type NotificationResponse = {
  id: string;
  type: string;
  title: string;
  message: string;
  sourceMessageId: string;
  orderId?: string | null;
  customerId?: string | null;
  createdAtUtc: string;
  readAtUtc?: string | null;
  isRead: boolean;
};

export async function getNotifications(unreadOnly = false, limit = 10) {
  const searchParams = new URLSearchParams({
    unreadOnly: String(unreadOnly),
    limit: String(limit),
  });

  return getJson<NotificationResponse[]>(`/api/notifications?${searchParams.toString()}`);
}

export async function markNotificationAsRead(id: string) {
  return postJson<NotificationResponse, Record<string, never>>(
    `/api/notifications/${id}/read`,
    {}
  );
}
