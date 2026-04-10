<template>
  <div>
    <h2>Deleted Assets</h2>

    <table border="1">
      <thead>
        <tr>
          <th>Asset Tag</th>
          <th>Category</th>
          <th>Department</th>
          <th>Action</th>
        </tr>
      </thead>

      <tbody>
        <tr v-for="a in assets" :key="a.id">
          <td>{{ a.assetTag }}</td>
          <td>{{ a.category }}</td>
          <td>{{ a.departmentName }}</td>

          <td>
            <button @click="restore(a.id)">
              Restore
            </button>
          </td>

        </tr>
      </tbody>
    </table>

  </div>
</template>

<script setup>
import { ref, onMounted } from "vue"
import axios from "axios"

const assets = ref([])

const loadDeleted = async () => {
    const res = await axios.get("https://localhost:7172/api/assets/deleted")
    assets.value = res.data.items
}

const restore = async (id) => {

    await axios.put(`https://localhost:7172/api/assets/restore/${id}`)

    alert("Restore thành công")

    loadDeleted()
}

onMounted(() => {
    loadDeleted()
})
</script>
