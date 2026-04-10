<template>
  <section class="request-history-page">
    <RequestHistoryTable :requests="requests"
                         :loading="loading"
                         :error-message="errorMessage" />
  </section>
</template>

<script setup>
  import { onMounted, ref } from "vue"
  import RequestHistoryTable from "../components/requests/RequestHistoryTable.vue"
  import { getEmployeeRequestHistory } from "../services/employeeRequest.service"

  const requests = ref([])
  const loading = ref(false)
  const errorMessage = ref("")

  onMounted(() => {
    fetchRequestHistory()
  })

  async function fetchRequestHistory() {
    loading.value = true
    errorMessage.value = ""

    try {
      const data = await getEmployeeRequestHistory()
      console.log("Request history data:", data)
      requests.value = Array.isArray(data) ? data : []
    } catch (error) {
      console.error("Failed to load request history:", error)
      console.error("History response:", error?.response?.data)

      errorMessage.value =
        error?.response?.data?.message ||
        "Failed to load request history."
    } finally {
      loading.value = false
    }
  }
</script>

<style scoped>
  .request-history-page {
    width: 100%;
  }
</style>
