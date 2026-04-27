const API_URL = process.env.NEXT_PUBLIC_API_URL;

export const getToken = () =>
  typeof window !== "undefined" ? localStorage.getItem("passenger_token") : null;

export const api = {
  get: async (path: string) => {
    const res = await fetch(`${API_URL}${path}`, {
      headers: { Authorization: `Bearer ${getToken()}` },
    });
    if (!res.ok) throw new Error(`HTTP ${res.status}`);
    return res.json();
  },

  post: async (path: string, body?: unknown) => {
    const res = await fetch(`${API_URL}${path}`, {
      method: "POST",
      headers: {
        Authorization: `Bearer ${getToken()}`,
        "Content-Type": "application/json",
      },
      body: body ? JSON.stringify(body) : undefined,
    });
    if (!res.ok) throw new Error(`HTTP ${res.status}`);
    return res.json();
  },

  put: async (path: string, body?: unknown) => {
    const res = await fetch(`${API_URL}${path}`, {
      method: "PUT",
      headers: {
        Authorization: `Bearer ${getToken()}`,
        "Content-Type": "application/json",
      },
      body: body ? JSON.stringify(body) : undefined,
    });
    if (!res.ok) throw new Error(`HTTP ${res.status}`);
    return res.json();
  },
};