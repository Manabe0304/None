<template>
  <div class="maintenance-container">
    <div class="header-section">
      <h2>Maintenance Records</h2>
      <div class="controls">
        <div class="search-box">
          <i class="fas fa-search"></i>
          <input v-model="search" @input="fetchData" placeholder="Search records..." />
        </div>
        <button class="btn btn-add" @click="openModal">
          <span>+</span> Add New Record
        </button>
      </div>
    </div>

    <div class="table-wrapper">
      <table class="styled-table">
        <thead>
          <tr>
            <th>Asset</th>
            <th>Vendor</th>
            <th>Status</th>
            <th>Outcome</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in records" :key="item.id">
            <td><strong>{{ item.assetTag }}</strong></td>
            <td>{{ item.vendorName }}</td>
            <td>
              <span :class="['status-badge', item.status.toLowerCase()]">
                {{ item.status }}
              </span>
            </td>
            <td>{{ item.outcome || 'N/A' }}</td>
          </tr>
          <tr v-if="records.length === 0">
            <td colspan="4" class="no-data">No records found.</td>
          </tr>
        </tbody>
      </table>
    </div>

    <div class="pagination">
      <button @click="changePage(-1)" :disabled="page <= 1" class="btn-page">Prev</button>
      <span class="page-info">Page {{ page }}</span>
      <button @click="changePage(1)" :disabled="records.length < 10" class="btn-page">Next</button>
    </div>

    <Transition name="fade">
      <div v-if="showModal" class="modal-overlay" @click.self="showModal = false">
        <div class="modal-content">
          <div class="modal-header">
            <h3>Add New Maintenance Record</h3>
            <button class="close-btn" @click="showModal = false">&times;</button>
          </div>

          <div class="modal-body">
            <div class="form-group">
              <label>Select Asset</label>
              <select v-model="form.assetId">
                <option :value="null" disabled>-- Choose Asset --</option>
                <option v-for="a in assets" :key="a.id" :value="a.id">{{ a.name }}</option>
              </select>
            </div>

            <div class="form-group">
              <label>Select Vendor</label>
              <select v-model="form.vendorId">
                <option :value="null" disabled>-- Choose Vendor --</option>
                <option v-for="v in vendors" :key="v.id" :value="v.id">{{ v.name }}</option>
              </select>
            </div>

            <div class="form-group">
              <label>Description</label>
              <textarea v-model="form.description" placeholder="Enter details..." rows="3"></textarea>
            </div>

            <div class="form-group">
              <label>Estimated Cost ($)</label>
              <input v-model="form.estimatedCost" type="number" placeholder="0.00" />
            </div>
            <div class="row">
              <div class="form-group col">
                <label>Start Date</label>
                <input v-model="form.startedAt" type="date" />
              </div>

              <div class="form-group col">
                <label>Completion Date (Est.)</label>
                <input v-model="form.completedAt" type="date" />
              </div>
            </div>
          </div>

          <div class="modal-footer">
            <button class="btn btn-cancel" @click="showModal = false">Cancel</button>
            <button class="btn btn-save" @click="create" :disabled="loading">
              {{ loading ? 'Saving...' : 'Save Record' }}
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </div>
</template>
<script>
  import axios from "axios";

  export default {
    data() {
      return {
        records: [],
        page: 1,
        search: "",
        showModal: false,
        loading: false,
        vendors: [],
        assets: [],
        form: {
          assetId: null,
          vendorId: null,
          description: "",
          estimatedCost: null,
          startedAt: null,
          completedAt: null
        }
      };
    },
    // Gộp tất cả các hàm vào một khối methods duy nhất
    methods: {
      async fetchData() {
        try {
          const res = await axios.get("/api/maintenance", {
            params: { search: this.search, page: this.page }
          });
          this.records = res.data.data;
        } catch (e) {
          console.error("Fetch error", e);
        }
      },

      async openModal() {
        this.showModal = true;
        if (this.vendors.length === 0 || this.assets.length === 0) {
          try {
            const [v, a] = await Promise.all([
              axios.get("/api/maintenance/vendors"),
              axios.get("/api/maintenance/assets")
            ]);
            this.vendors = v.data;
            this.assets = a.data;
          } catch (e) {
            console.error("Error loading dropdown data", e);
          }
        }
      },

      async create() {
        // Validation cơ bản
        if (!this.form.assetId || !this.form.vendorId) {
          return alert("Please select both Asset and Vendor.");
        }

        // Kiểm tra logic ngày tháng (tùy chọn)
        if (this.form.startedAt && this.form.completedAt) {
          if (new Date(this.form.startedAt) > new Date(this.form.completedAt)) {
            return alert("Start date cannot be after Completion date.");
          }
        }

        this.loading = true;
        try {
          await axios.post("/api/maintenance", this.form);
          alert("Record saved successfully!");
          this.showModal = false;
          this.resetForm();
          this.fetchData();
        } catch (e) {
          console.error("Create error", e);
          alert("Failed to save record.");
        } finally {
          this.loading = false;
        }
      },

      resetForm() {
        this.form = {
          assetId: null,
          vendorId: null,
          description: "",
          estimatedCost: null,
          startedAt: null,
          completedAt: null
        };
      },

      changePage(step) {
        this.page += step;
        this.fetchData();
      }
    },
    mounted() {
      this.fetchData();
    }
  };
</script>

<style scoped>
  /* Tổng quan */
  .maintenance-container {
    padding: 20px;
    font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
    color: #333;
    background-color: #f8f9fa;
    min-height: 100vh;
  }

  /* Header & Controls */
  .header-section {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 25px;
  }

  .controls {
    display: flex;
    gap: 15px;
  }

  .search-box input {
    padding: 10px 15px;
    border-radius: 8px;
    border: 1px solid #ddd;
    width: 250px;
    outline: none;
    transition: border-color 0.3s;
  }

    .search-box input:focus {
      border-color: #3498db;
    }

  /* Table */
  .table-wrapper {
    background: white;
    border-radius: 12px;
    box-shadow: 0 4px 6px rgba(0,0,0,0.05);
    overflow: hidden;
  }

  .styled-table {
    width: 100%;
    border-collapse: collapse;
  }

    .styled-table th {
      background-color: #f1f3f5;
      padding: 15px;
      text-align: left;
      font-weight: 600;
      color: #495057;
    }

    .styled-table td {
      padding: 15px;
      border-bottom: 1px solid #eee;
    }

    .styled-table tr:hover {
      background-color: #fcfcfc;
    }

  /* Status Badge */
  .status-badge {
    padding: 4px 10px;
    border-radius: 20px;
    font-size: 12px;
    font-weight: bold;
    text-transform: uppercase;
  }

    .status-badge.pending {
      background: #fff3cd;
      color: #856404;
    }

    .status-badge.completed {
      background: #d4edda;
      color: #155724;
    }

  /* Modal Overlay */
  .modal-overlay {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: rgba(0, 0, 0, 0.5);
    display: flex;
    justify-content: center;
    align-items: center;
    z-index: 1000;
    backdrop-filter: blur(4px);
  }

  /* Modal Content */
  .modal-content {
    background: white;
    width: 500px;
    border-radius: 12px;
    box-shadow: 0 10px 25px rgba(0,0,0,0.2);
    overflow: hidden;
    animation: slideIn 0.3s ease-out;
  }

  .modal-header {
    padding: 20px;
    background: #f8f9fa;
    border-bottom: 1px solid #eee;
    display: flex;
    justify-content: space-between;
    align-items: center;
  }

  .modal-body {
    padding: 20px;
  }

  .form-group {
    margin-bottom: 15px;
  }

    .form-group label {
      display: block;
      margin-bottom: 8px;
      font-weight: 600;
    }

    .form-group input, .form-group select, .form-group textarea {
      width: 100%;
      padding: 10px;
      border: 1px solid #ddd;
      border-radius: 6px;
      box-sizing: border-box;
    }

  .modal-footer {
    padding: 15px 20px;
    background: #f8f9fa;
    display: flex;
    justify-content: flex-end;
    gap: 10px;
  }

  /* Buttons */
  .btn {
    padding: 10px 20px;
    border-radius: 8px;
    border: none;
    cursor: pointer;
    font-weight: 500;
    transition: transform 0.1s, opacity 0.2s;
  }

    .btn:active {
      transform: scale(0.98);
    }

  .btn-add {
    background: #2ecc71;
    color: white;
  }

  .btn-save {
    background: #3498db;
    color: white;
  }

  .btn-cancel {
    background: #e0e0e0;
    color: #666;
  }

  /* Pagination */
  .pagination {
    margin-top: 20px;
    display: flex;
    justify-content: center;
    align-items: center;
    gap: 15px;
  }

  /* Animations */
  @keyframes slideIn {
    from {
      transform: translateY(-30px);
      opacity: 0;
    }

    to {
      transform: translateY(0);
      opacity: 1;
    }
  }

  .fade-enter-active, .fade-leave-active {
    transition: opacity 0.3s;
  }

  .fade-enter-from, .fade-leave-to {
    opacity: 0;
  }
</style>
