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
    <div className="notification-root">
      <button
        type="button"
        onClick={() => setIsOpen((current) => !current)}
        className={`button compact notification-button${unreadCount > 0 ? " has-unread" : ""}`}
      >
        Notifications {unreadCount > 0 ? `(${unreadCount})` : ""}
      </button>

      {toast && (
        <div role="status" className="notification-toast">
          <strong style={{ display: "block", marginBottom: "4px" }}>{toast.title}</strong>
          <span>{toast.message}</span>
        </div>
      )}

      {isOpen && (
        <div className="notification-popover">
          <div className="panel-header">
            <strong>Recent notifications</strong>
            <button
              type="button"
              onClick={() => void loadNotifications()}
              className="button secondary compact"
            >
              Refresh
            </button>
          </div>

          {error && (
            <div className="feedback error" style={{ borderRadius: 0 }}>
              {error}
            </div>
          )}

          {notifications.length === 0 ? (
            <div className="empty-state">No notifications yet.</div>
          ) : (
            <div style={{ maxHeight: "360px", overflowY: "auto" }}>
              {notifications.map((notification) => (
                <div
                  key={notification.id}
                  className={`notification-item${notification.isRead ? "" : " unread"}`}
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
                      className="button primary compact"
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
