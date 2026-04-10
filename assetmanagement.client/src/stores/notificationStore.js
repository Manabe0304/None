import { reactive, readonly } from "vue"

const state = reactive({
  notifications: []
})

let notificationSeed = 0

function normalizeType(type = "info") {
  const value = String(type || "info").toLowerCase()

  if (["success", "error", "warning", "info"].includes(value)) {
    return value
  }

  return "info"
}

export function showNotification(title, message, type = "info", options = {}) {
  const id = ++notificationSeed
  const item = {
    id,
    title: title || "Notification",
    message: message || "",
    type: normalizeType(type),
    duration: typeof options.duration === "number" ? options.duration : 3200
  }

  state.notifications.push(item)

  if (item.duration > 0) {
    window.setTimeout(() => removeNotification(id), item.duration)
  }

  return id
}

export function removeNotification(id) {
  const index = state.notifications.findIndex((item) => item.id === id)
  if (index !== -1) {
    state.notifications.splice(index, 1)
  }
}

export function useNotificationStore() {
  return {
    notifications: readonly(state.notifications),
    removeNotification
  }
}

export function patchWindowAlert() {
  if (typeof window === "undefined") return
  if (window.__assetMgmtAlertPatched) return

  window.__assetMgmtAlertPatched = true
  const nativeAlert = window.alert?.bind(window)
  window.__nativeAlert = nativeAlert

  window.alert = (message) => {
    showNotification("Notice", String(message ?? ""), "info")
  }
}
