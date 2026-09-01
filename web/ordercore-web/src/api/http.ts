import { getAccessToken } from "../auth/session";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "https://localhost:7171";

function createHeaders(headers?: HeadersInit) {
  const accessToken = getAccessToken();
  const requestHeaders = new Headers(headers);

  if (accessToken) {
    requestHeaders.set("Authorization", `Bearer ${accessToken}`);
  }

  return requestHeaders;
}

async function parseResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    let errorMessage = `Request failed with status ${response.status}`;

    try {
      const errorBody = await response.json();
      errorMessage =
        errorBody?.message ??
        errorBody?.detail ??
        errorBody?.Detail ??
        errorBody?.title ??
        errorBody?.Title ??
        errorMessage;
    } catch {
      // ignore json parse errors
    }

    throw new Error(errorMessage);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return response.json() as Promise<T>;
}

export async function getJson<T>(path: string): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    method: "GET",
    headers: createHeaders({
      Accept: "application/json",
    }),
  });

  return parseResponse<T>(response);
}

export async function postJson<TResponse, TBody>(
  path: string,
  body: TBody
): Promise<TResponse> {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    method: "POST",
    headers: createHeaders({
      "Content-Type": "application/json",
      Accept: "application/json",
    }),
    body: JSON.stringify(body),
  });

  return parseResponse<TResponse>(response);
}
