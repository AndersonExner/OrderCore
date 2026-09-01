import { postJson } from "./http";

export const authRoles = {
  admin: "Admin",
  sales: "Sales",
  finance: "Finance",
  viewer: "Viewer",
} as const;

export type AuthenticatedUserResponse = {
  id: string;
  userName: string;
  email: string;
  role: string;
};

export type AuthResponse = {
  accessToken: string;
  expiresAtUtc: string;
  user: AuthenticatedUserResponse;
};

export type LoginRequest = {
  userNameOrEmail: string;
  password: string;
};

export type RegisterUserRequest = {
  userName: string;
  email: string;
  password: string;
  role: string;
};

export async function login(request: LoginRequest) {
  return postJson<AuthResponse, LoginRequest>("/api/auth/login", request);
}

export async function registerUser(request: RegisterUserRequest) {
  return postJson<AuthResponse, RegisterUserRequest>("/api/auth/register", request);
}
