
const API_URL = "/api";

function clearAuth() {
    localStorage.removeItem("token");
    localStorage.removeItem("role");
    localStorage.removeItem("code");
}

async function checkResponse(response) {
    if (response.status === 401) {
        clearAuth();
        throw new Error("Unauthorized. Please verify the access code again.");
    }

    if (response.status === 403)
        throw new Error("Special access key required.");

    if (!response.ok) {
        const message = await response.text();
        throw new Error(message || `Request failed. Status: ${response.status}`);
    }
}

export async function verifyAccess(code) {
    const challengeResponse = await fetch(`${API_URL}/auth/challenge`, { method: "POST" });
    if (!challengeResponse.ok)
        throw new Error("Failed to obtain challenge.");

    const { challenge } = await challengeResponse.json();
    const encoder = new TextEncoder();
    const key = await crypto.subtle.importKey("raw", encoder.encode(code), { name: "HMAC", hash: "SHA-256" }, false, ["sign"]);
    const signature = await crypto.subtle.sign("HMAC", key, Uint8Array.from(atob(challenge), c => c.charCodeAt(0)));
    const response = btoa(String.fromCharCode(...new Uint8Array(signature)));

    const verifyResponse = await fetch(`${API_URL}/auth/verify`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ challenge, response })
    });

    if (verifyResponse.status === 401)
        return null;

    if (!verifyResponse.ok)
        throw new Error("Failed to verify access code.");

    const { token, role } = await verifyResponse.json();
    localStorage.setItem("token", token);
    if (role)
        localStorage.setItem("role", role);

    return token;
}

export async function getList(type = "rom") {
    const token = localStorage.getItem("token");
    if (!token)
        throw new Error("Missing access token.");

    const url = new URL(`${API_URL}/File/getlist`);
    url.searchParams.set("type", type);

    const response = await fetch(url, { headers: { Authorization: `Bearer ${token}` } });
    await checkResponse(response);

    return response.json();
}

export async function getFile(type, id) {
    const token = localStorage.getItem("token");
    if (!token)
        throw new Error("Missing access token.");

    const response = await fetch(`${API_URL}/File/file/${encodeURIComponent(type)}/${id}`, { headers: { Authorization: `Bearer ${token}` } });
    await checkResponse(response);

    const blob = await response.blob();

    return new File([blob], `file-${id}`, { type: "application/octet-stream" });
}

export async function uploadFile(type, file) {
    const token = localStorage.getItem("token");
    if (!token)
        throw new Error("Missing access token.");

    const formData = new FormData();
    formData.append("file", file);

    const response = await fetch(`${API_URL}/File/file/${encodeURIComponent(type)}`, {
        method: "POST",
        headers: { Authorization: `Bearer ${token}` },
        body: formData
    });

    await checkResponse(response);
    return response.json();
}

export async function deleteFile(type, id) {
    const token = localStorage.getItem("token");
    if (!token)
        throw new Error("Missing access token.");

    const response = await fetch(`${API_URL}/File/file/${encodeURIComponent(type)}/${id}`, {
        method: "DELETE",
        headers: { Authorization: `Bearer ${token}` }
    });

    await checkResponse(response);
}
