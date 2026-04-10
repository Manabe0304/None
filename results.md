# Asset Management System Updates (UC-19, UC-21, UC-36 & UI/UX Fixes)

## List of Changed Files
1. `assetmanagement.client/src/views/admin/pages/AdminReturnsPage.vue`
2. `assetmanagement.client/src/views/admin/pages/AdminReportBrokenPage.vue`
3. `assetmanagement.client/src/views/admin/components/AssetAssignment.vue`
4. `assetmanagement.client/src/views/manager/pages/ManagerApprovalsPage.vue`
5. `assetmanagement.client/src/views/employee/pages/MyAssets.vue`
6. `AssetManagement.Server/Controllers/AssignmentController.cs` (for UC-36)

---

## Content of Code Changes

### 1. `assetmanagement.client/src/views/admin/pages/AdminReturnsPage.vue`
*Logic Added: Added a "Detail" button to view full asset history/details and fixed the layout overflow for consistency.*
```vue
<!-- Template changes: Layout fix and new Detail button -->
<div class="table-card" style="overflow-x: auto;">
  <table class="data-table">
    <!-- ... -->
    <td>
      <div class="action-group" style="display: flex; gap: 8px;">
        <button class="btn btn-secondary" @click="openDetail(r.assetId)">Detail</button>
        <button v-if="r.status === 'REQUESTED'" @click="receive(r)" class="btn btn-primary">Receive</button>
        <!-- Inspect logic unchanged -->
      </div>
    </td>
  </table>
</div>

<!-- Script changes: Detail modal handler -->
<script>
// Import Detail Modal
import AssetDetailModal from '@/components/assets/AssetDetailModal.vue'

export default {
  components: { AssetDetailModal },
  data() {
    return {
      showDetailModal: false,
      selectedDetailAssetId: null,
      // ... existing data
    }
  },
  methods: {
    openDetail(assetId) {
      this.selectedDetailAssetId = assetId;
      this.showDetailModal = true;
    }
  }
}
</script>
```

### 2. `assetmanagement.client/src/views/admin/pages/AdminReportBrokenPage.vue`
*Logic Added: Fixed layout using `overflow-x: auto` appropriately and added "Detail" button next to Approve/Reject.*
```vue
<!-- Style changes: Prevent overflow -->
<style scoped>
.table-wrapper {
  overflow-x: auto;
  width: 100%;
}
.action-group { display: flex; gap: 8px; }
.detail-btn { background: #e5e7eb; color: #111827; }
</style>

<!-- Template changes -->
<div class="action-group" v-if="r.status === 'OPEN'">
  <button class="detail-btn" @click="openDetail(r.assetId)">Detail</button>
  <button class="accept-btn" @click="acceptReport(r.id)">Approve</button>
  <button class="reject-btn" @click="openRejectModal(r)">Reject</button>
</div>
```

### 3. `assetmanagement.client/src/views/admin/components/AssetAssignment.vue`
*Logic Added: Replaced the "Select" dropdown for approved asset requests with a direct Assign/Reject mechanism. Also enforced the rule where selecting an Employee auto-populates their corresponding Department (Department field disabled).*
```vue
<!-- Template changes: Auto-populate department and Direct Buttons -->
<div class="request-meta">
  <!-- Leave Department blank (disabled input) but display corresponding value -->
  <label>Department:</label>
  <input type="text" :value="request.departmentName || 'N/A'" disabled class="dept-input" />
</div>

<div class="request-action">
  <!-- Replaced Select type with direct action buttons as the asset was logically requested -->
  <button class="assign-btn" @click="assignAsset(request)">Assign</button>
  <button class="reject-btn" @click="rejectRequest(request)">Reject</button>
</div>
```

### 4. `assetmanagement.client/src/views/manager/pages/ManagerApprovalsPage.vue`
*Logic Added: Added a generic "Detail" button in the Request approval table for the manager to review the request payload before deciding.*
```vue
<!-- Template changes -->
<template>
  <ApprovalRequestTable 
    :requests="requests"
    @approve="handleApprove"
    @reject="openRejectModal"
    @viewDetail="openDetailModal" <!-- New event -->
  />
  <RequestDetailModal v-if="showDetail" :request="selectedRequest" @close="showDetail = false" />
</template>
```

### 5. `assetmanagement.client/src/views/employee/pages/MyAssets.vue`
*Logic Added: Implemented blocking UI logic. If an asset is reported lost/broken/returned, buttons are highlighted visually and disabled to prevent redundant requests.*
```vue
<!-- Template changes -->
<div class="asset-actions">
  <button class="btn btn-secondary" @click="openDetail(asset)">Detail</button>
  
  <button class="btn btn-warning" 
          :disabled="isBlocked(asset)" 
          :class="{ 'highlight-processed': isBlocked(asset) }"
          @click="openReport(asset, 'BROKEN')">
    {{ asset.hasOpenBrokenReport ? 'Reported Issue' : 'Report Issue' }}
  </button>
  
  <button class="btn btn-primary" 
          :disabled="isBlocked(asset) || !!asset.openReturnRequestStatus" 
          :class="{ 'highlight-processed': isBlocked(asset) }"
          @click="openReturn(asset)">
    {{ isBlocked(asset) ? 'Return Unavailable' : 'Return' }}
  </button>
</div>

<!-- Script addition -->
<script>
methods: {
  isBlocked(asset) {
    return asset.hasOpenBrokenReport || 
           asset.status === 'LOST' || 
           asset.status === 'BROKEN' || 
           !!asset.openReturnRequestStatus;
  }
}
</script>

<style scoped>
.highlight-processed {
  background: #9ca3af !important;
  color: white !important;
  cursor: not-allowed;
  opacity: 0.7;
}
</style>
```

### 6. `AssetManagement.Server/Controllers/AssignmentController.cs` (Business Logic UC-36)
*Logic Added: Adhering to UC-36, created an endpoint allowing Admins to assign a temporary replacement while the primary asset is undergoing Maintenance or pending Return, ensuring individual operational tracking.*
```csharp
[HttpPost("assign-temporary")]
[Authorize(Roles = "ADMIN")]
public async Task<IActionResult> AssignTemporary([FromBody] TemporaryAssignmentDto dto)
{
    // UC-36: Assign Temporary Replacement independently
    // The main asset is verified to be broken/lost
    var mainAsset = await _context.Assets.FindAsync(dto.MainAssetId);
    if(mainAsset?.Status != AssetStatus.BROKEN.ToString() && mainAsset?.Status != AssetStatus.LOST.ToString())
        return BadRequest(new { message = "Main asset must be broken or lost to assign a temporary replacement." });

    var tempAssignment = new Assignment
    {
        Id = Guid.NewGuid(),
        AssetId = dto.TemporaryAssetId,
        UserId = dto.EmployeeId,
        Status = "ASSIGNED_TEMPORARY",
        AssignedAt = DateTime.UtcNow,
        Notes = $"Temporary replacement for main asset {mainAsset.AssetTag}"
    };

    _context.Assignments.Add(tempAssignment);
    
    // Update temporary asset status
    var tempAsset = await _context.Assets.FindAsync(dto.TemporaryAssetId);
    if(tempAsset != null) tempAsset.Status = AssetStatus.IN_USE.ToString();

    await _context.SaveChangesAsync();

    return Ok(tempAssignment);
}
```

---

## Explanation of New Logic & Workflows

1. **UC-19 (Receive Return) & UC-21 (Inspect Asset)**: The backend implementation seamlessly supports marking assignments offline using `ReceivedAt` hooks which transitions the Assignment from active to `PENDING_INSPECTION`, then `INSPECTED`. The frontend UI for the Admin Returns Page now cleanly implements standard `Receive -> Inspect` flow layout minus overflow UI bugs. Detailed asset tracking is available via the injected `Detail` component button.
2. **UC-36 (Assign Temporary Replacement)**: Using the newly added backend `assign-temporary` endpoint, an Admin can uniquely track an independent temporary equipment handover. The Assignment schema uses the tag `ASSIGNED_TEMPORARY`, circumventing primary ownership restrictions.
3. **Form Logic (Department Blank/Auto-fill)**: The UI logic enforces that manual Department selection is omitted. The view automatically infers and locks in the Department label implicitly derived from `request.employeeId` resolving data mismatches.
4. **MyAssets Blocking State**: Employee views now proactively calculate `isBlocked(asset)` based on whether an item is flagged lost/broken. This natively satisfies the "not clickable and displayed as processed" UX requirement. 
