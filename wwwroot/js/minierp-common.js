
"use strict";

// 將不可信文字轉成安全的 HTML 文字內容
function escapeHtml(value) {
    return String(value ?? "")
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#39;");
}

// 統一處理 API 請求
async function apiFetch(url, options = {}) {
    const method = (options.method || "GET").toUpperCase();
    const headers = new Headers(options.headers || {});

    if (!["GET", "HEAD", "OPTIONS", "TRACE"].includes(method)) {
        const token = document.querySelector(
            'meta[name="csrf-token"]'
        )?.getAttribute("content");

        if (!token) {
            throw new Error("找不到 CSRF Token，請重新整理頁面。");
        }

        headers.set("X-CSRF-TOKEN", token);
    }

    const response = await fetch(url, {
        ...options,
        headers,
        credentials: "same-origin"
    });

    if (response.status === 401) {
        window.location.href = "/Account/Login";
        throw new Error("登入已失效，請重新登入。");
    }

    if (response.status === 403) {
        throw new Error("沒有執行此操作的權限。");
    }

    return response;
}

// 統一解析 API 錯誤訊息
async function getErrorMessage(response) {
    let data;

    try {
        data = await response.json();
    } catch {
        return `操作失敗（HTTP ${response.status}）。`;
    }

    if (typeof data === "string") {
        return data;
    }

    if (Array.isArray(data)) {
        return data
            .map(e => e.description || e.code)
            .filter(Boolean)
            .join("；") || "操作失敗。";
    }

    if (data.errors) {
        const errors = data.errors;
        const messages = [];

        for (const [field, fieldErrors] of Object.entries(errors)) {
            const key = field.toLowerCase();

            if (key.includes("stockquantity")) {
                messages.push(
                    "庫存數量必須是 0 到 2,147,483,647 之間的整數。"
                );
                continue;
            }

            if (key.includes("price") || key.includes("unitprice")) {
                messages.push(
                    "金額必須介於 0 到 9,999,999,999,999,999.99 之間。"
                );
                continue;
            }

            if (key === "dto") {
                // DTO 反序列化失敗時的附帶錯誤，
                // 已由具體欄位錯誤處理。
                continue;
            }

            for (const message of fieldErrors) {
                if (
                    message.includes("could not be converted") ||
                    message.includes("The JSON value")
                ) {
                    messages.push("輸入資料格式不正確，請檢查數值。");
                } else {
                    messages.push(message);
                }
            }
        }

        if (messages.length > 0) {
            return [...new Set(messages)].join("；");
        }

        return "輸入資料格式不正確，請檢查各欄位。";
    }

    return data.message ||
        data.detail ||
        data.title ||
        `操作失敗（HTTP ${response.status}）。`;
}

