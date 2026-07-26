import { useEffect, useRef, useState } from "react";

import {
  getNotifications,
  markNotificationAsRead,
} from "../api/notifications";
import type { NotificationResponse } from "../api/notifications";

const pollIntervalMs = 5000;

export default function NotificationsMenu() {
  const [notifications, setNotifications] = useState<NotificationResponse[]>([]);
  const [isOpen, setIsOpen] = useState(false);
  const [toast, setToast] = useState<NotificationResponse | null>(null);
  const [error, setError] = useState("");
  const knownNotificationIds = useRef<Set<string>>(new Set());
  const hasLoadedOnce = useRef(false);

  const unreadCount = notifications.filter((notification) => !notification.isRead).length;

  async function loadNotifications() {
    try {
      const result = await getNotifications(false, 10);

      if (hasLoadedOnce.current) {
        const newNotification = result.find(
          (notification) =>
            !notification.isRead && !knownNotificationIds.current.has(notification.id)
        );

        if (newNotification) {
          setToast(newNotification);
        }
      }

      knownNotificationIds.current = new Set(result.map((notification) => notification.id));
      hasLoadedOnce.current = true;
      setNotifications(result);
      setError("");
    } catch (loadError) {
      setError(loadError instanceof Error ? loadError.message : "Failed to load notifications.");
    }
  }

  useEffect(() => {
    void loadNotifications();

    const intervalId = window.setInterval(() => {
      void loadNotifications();
    }, pollIntervalMs);

    return () => window.clearInterval(intervalId);
  }, []);

  useEffect(() => {
    if (!toast) {
      return;
    }

    const timeoutId = window.setTimeout(() => setToast(null), 6000);

    return () => window.clearTimeout(timeoutId);
  }, [toast]);

  async function handleMarkAsRead(notificationId: string) {
    try {
      const updatedNotification = await markNotificationAsRead(notificationId);

      setNotifications((current) =>
        current.map((notification) =>
          notification.id === updatedNotification.id ? updatedNotification : notification
        )
      );

      if (toast?.id === updatedNotification.id) {
        setToast(null);
      }
    } catch (markError) {
      setError(markError instanceof Error ? markError.message : "Failed to update notification.");
    }
  }

  return (
    <div style={{ position: "relative" }}>
      <button
        type="button"
        onClick={() => setIsOpen((current) => !current)}
        style={{
          background: unreadCount > 0 ? "#2563eb" : "#374151",
          color: "white",
          border: "1px solid rgba(255,255,255,0.25)",
          borderRadius: "8px",
          padding: "8px 10px",
          cursor: "pointer",
          minWidth: "118px",
          height: "36px",
        }}
      >
        Notifications {unreadCount > 0 ? `(${unreadCount})` : ""}
      </button>

      {toast && (
        <div
          role="status"
          style={{
            position: "absolute",
            top: "48px",
            right: 0,
            width: "320px",
            background: "#ecfdf5",
            color: "#064e3b",
            border: "1px solid #a7f3d0",
            borderRadius: "8px",
            boxShadow: "0 10px 24px rgba(0,0,0,0.16)",
            padding: "12px",
            zIndex: 20,
          }}
        >
          <strong style={{ display: "block", marginBottom: "4px" }}>{toast.title}</strong>
          <span>{toast.message}</span>
        </div>
      )}

      {isOpen && (
        <div
          style={{
            position: "absolute",
            top: "44px",
            right: 0,
            width: "360px",
            background: "white",
            color: "#111827",
            border: "1px solid #e5e7eb",
            borderRadius: "8px",
            boxShadow: "0 16px 32px rgba(0,0,0,0.18)",
            zIndex: 30,
            overflow: "hidden",
          }}
        >
          <div
            style={{
              padding: "12px",
              borderBottom: "1px solid #e5e7eb",
              display: "flex",
              justifyContent: "space-between",
              alignItems: "center",
            }}
          >
            <strong>Recent notifications</strong>
            <button
              type="button"
              onClick={() => void loadNotifications()}
              style={{
                border: "1px solid #d1d5db",
                background: "white",
                borderRadius: "6px",
                cursor: "pointer",
                padding: "6px 8px",
              }}
            >
              Refresh
            </button>
          </div>

          {error && (
            <div style={{ padding: "12px", color: "#991b1b", background: "#fee2e2" }}>
              {error}
            </div>
          )}

          {notifications.length === 0 ? (
            <div style={{ padding: "14px", color: "#6b7280" }}>No notifications yet.</div>
          ) : (
            <div style={{ maxHeight: "360px", overflowY: "auto" }}>
              {notifications.map((notification) => (
                <div
                  key={notification.id}
                  style={{
                    padding: "12px",
                    borderBottom: "1px solid #f3f4f6",
                    background: notification.isRead ? "white" : "#eff6ff",
                  }}
                >
                  <div style={{ display: "flex", justifyContent: "space-between", gap: "12px" }}>
                    <strong style={{ fontSize: "14px" }}>{notification.title}</strong>
                    <span style={{ color: "#6b7280", fontSize: "12px", whiteSpace: "nowrap" }}>
                      {new Date(notification.createdAtUtc).toLocaleTimeString()}
                    </span>
                  </div>
                  <p style={{ margin: "6px 0 8px", color: "#374151", fontSize: "13px" }}>
                    {notification.message}
                  </p>
                  {!notification.isRead && (
                    <button
                      type="button"
                      onClick={() => void handleMarkAsRead(notification.id)}
                      style={{
                        border: "none",
                        background: "#2563eb",
                        color: "white",
                        borderRadius: "6px",
                        cursor: "pointer",
                        padding: "6px 8px",
                      }}
                    >
                      Mark as read
                    </button>
                  )}
                </div>
              ))}
            </div>
          )}
        </div>
      )}
    </div>
  );
}
