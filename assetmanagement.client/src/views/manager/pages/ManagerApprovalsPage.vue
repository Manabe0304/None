<template>
  <section class="manager-approvals-page">
    <ApprovalRequestTable :requests="requests"
                          :loading="loading"
                          :error-message="errorMessage"
                          :action-loading-id="actionLoadingId"
                          @approve="handleApprove"
                          @reject="openRejectModal"
                          @viewDetail="openDetailModal" />

    <RejectReasonModal v-model="isRejectModalOpen"
                       :submitting="rejectSubmitting"
                       @submit="submitReject" />

    <RequestDetailModal v-if="showDetail" 
                        :request="selectedRequest" 
                        @close="showDetail = false" />
  </section>
</template>

<script setup>
  import { onMounted, ref } from 'vue'
  import ApprovalRequestTable from '../components/approvals/ApprovalRequestTable.vue'
  import RejectReasonModal from '../components/approvals/RejectReasonModal.vue'
  import RequestDetailModal from '../components/approvals/RequestDetailModal.vue'
  import {
    getApprovalRequests,
    approveApprovalRequest,
    rejectApprovalRequest
  } from '../services/managerApproval.service'
  import { APPROVAL_STATUS } from '../types/approval'

  const requests = ref([])
  const loading = ref(false)
  const errorMessage = ref('')
  const actionLoadingId = ref(null)

  const isRejectModalOpen = ref(false)
  const selectedRejectId = ref(null)
  const rejectSubmitting = ref(false)

  const showDetail = ref(false)
  const selectedRequest = ref(null)

  onMounted(() => {
    fetchApprovalRequests()
  })

  async function fetchApprovalRequests() {
    loading.value = true
    errorMessage.value = ''

    try {
      const data = await getApprovalRequests()
      requests.value = data.filter(item => item.status === APPROVAL_STATUS.PENDING)
    } catch (error) {
      console.error('Failed to load approval requests:', error)
      errorMessage.value = 'Failed to load approval requests.'
    } finally {
      loading.value = false
    }
  }

  async function handleApprove(id) {
    actionLoadingId.value = id
    errorMessage.value = ''

    try {
      await approveApprovalRequest(id)
      requests.value = requests.value.filter(item => item.id !== id)
    } catch (error) {
      console.error('Approve request failed:', error)
      errorMessage.value = 'Approve request failed.'
    } finally {
      actionLoadingId.value = null
    }
  }

  function openRejectModal(id) {
    selectedRejectId.value = id
    isRejectModalOpen.value = true
  }

  function openDetailModal(request) {
    selectedRequest.value = request
    showDetail.value = true
  }

  async function submitReject(reason) {
    if (!selectedRejectId.value) return

    rejectSubmitting.value = true
    actionLoadingId.value = selectedRejectId.value
    errorMessage.value = ''

    try {
      await rejectApprovalRequest(selectedRejectId.value, reason)
      requests.value = requests.value.filter(item => item.id !== selectedRejectId.value)
      isRejectModalOpen.value = false
      selectedRejectId.value = null
    } catch (error) {
      console.error('Reject request failed:', error)
      errorMessage.value = 'Reject request failed.'
    } finally {
      rejectSubmitting.value = false
      actionLoadingId.value = null
    }
  }
</script>

<style scoped>
  .manager-approvals-page {
    padding: 0;
  }
</style>
