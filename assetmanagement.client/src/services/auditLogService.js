import api from "@/services/axiosConfig";

const API_URL = "/auditlogs";

const extractArray = (payload) => {
  if (Array.isArray(payload)) return payload;
  if (Array.isArray(payload?.data)) return payload.data;
  if (Array.isArray(payload?.items)) return payload.items;
  if (Array.isArray(payload?.result)) return payload.result;
  return [];
};

const auditLogService = {
  async getAll() {
    const response = await api.get(API_URL);
    return extractArray(response?.data);
  },
};

export default auditLogService;
