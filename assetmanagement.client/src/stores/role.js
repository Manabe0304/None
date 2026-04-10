import { defineStore } from 'pinia';

export const useRoleStore = defineStore('role', {
  state: () => ({
    currentRole: 'admin'
  }),
  actions: {
    setRole(role) {
      this.currentRole = role;
    }
  }
});