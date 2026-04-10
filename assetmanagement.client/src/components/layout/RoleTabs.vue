<template>
  <div class="tabs">
    <button @click="setRole('admin')">Admin</button>
    <button @click="setRole('manager')">Manager</button>
    <button @click="setRole('employee')">Employee</button>
  </div>
</template>

<script>
import { useRoleStore } from '../../stores/role';

export default {
  setup() {
    const roleStore = useRoleStore();
    const setRole = (role) => {
      roleStore.setRole(role);
    };
    return { setRole };
  }
};
</script>

<style>
.tabs {
  display: flex;
  background: #222;
}

.tabs button {
  flex: 1;
  padding: 14px;
  border: none;
  cursor: pointer;
  background: #333;
  color: white;
  font-size: 15px;
  transition: 0.3s;
}

.tabs button:hover {
  background: #555;
}
</style>