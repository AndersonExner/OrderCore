import type { AuthResponse, AuthenticatedUserResponse } from "../api/auth";

const sessionStorageKey = "ordercore.auth";

export function saveAuthSession(auth: AuthResponse) {
  localStorage.setItem(sessionStorageKey, JSON.stringify(auth));
}

export function getAuthSession(): AuthResponse | null {
  const rawSession = localStorage.getItem(sessionStorageKey);

  if (!rawSession) {
    return null;
  }

  try {
    const session = JSON.parse(rawSession) as AuthResponse;

    if (new Date(session.expiresAtUtc) <= new Date()) {
      clearAuthSession();
      return null;
    }

    return session;
  } catch {
    clearAuthSession();
    return null;
  }
}

export function clearAuthSession() {
  localStorage.removeItem(sessionStorageKey);
}

export function getAccessToken() {
  return getAuthSession()?.accessToken ?? "";
}

export function getCurrentUser(): AuthenticatedUserResponse | null {
  return getAuthSession()?.user ?? null;
}

export function hasAnyRole(...roles: string[]) {
  const user = getCurrentUser();

  if (!user) {
    return false;
  }

  return roles.includes(user.role);
}
