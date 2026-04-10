<template>
  <div class="vendor-container">
    <div class="header-section">
      <h2>Vendor Management</h2>
      <div class="controls">
        <div class="search-box">
          <input v-model="search" @input="fetchData" placeholder="Search vendors..." />
        </div>
        <button class="btn btn-primary" @click="openModal()">+ Add New Vendor</button>
      </div>
    </div>

    <div class="table-card">
      <table class="styled-table">
        <thead>
          <tr>
            <th>Name</th>
            <th>Email</th>
            <th>Phone</th>
            <th class="text-center">Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="v in vendors" :key="v.id">
            <td class="vendor-name">{{ v.name }}</td>
            <td>{{ v.contactEmail }}</td>
            <td>{{ v.phone }}</td>
            <td class="text-center">
              <button class="btn-icon edit" @click="openModal(v)" title="Edit">Edit</button>
              <button class="btn-icon delete" @click="remove(v.id)" title="Delete">Delete</button>
            </td>
          </tr>
          <tr v-if="vendors.length === 0">
            <td colspan="4" class="no-data">No vendors found.</td>
          </tr>
        </tbody>
      </table>
    </div>

    <div class="pagination">
      <button class="btn-page" @click="prev" :disabled="page==1">Prev</button>
      <span class="page-number">Page {{ page }}</span>
      <button class="btn-page" @click="next" :disabled="vendors.length < 10">Next</button>
    </div>

    <Transition name="fade">
      <div v-if="showModal" class="modal-overlay" @click.self="showModal = false">
        <div class="modal-content">
          <div class="modal-header">
            <h3>{{ form.id ? "Edit Vendor" : "Add New Vendor" }}</h3>
            <button class="close-btn" @click="showModal = false">&times;</button>
          </div>
          <div class="modal-body">
            <div class="form-group">
              <label>Vendor Name</label>
              <input v-model="form.name" placeholder="Enter name" />
            </div>
            <div class="form-group">
              <label>Contact Email</label>
              <input v-model="form.contactEmail" placeholder="email@example.com" />
            </div>
            <div class="form-group">
              <label>Phone Number</label>
              <input v-model="form.phone" placeholder="Enter phone number" />
            </div>
          </div>
          <div class="modal-footer">
            <button class="btn btn-secondary" @click="showModal = false">Cancel</button>
            <button class="btn btn-primary" @click="save">Save Changes</button>
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
        vendors: [],
        search: "",
        page: 1,
        showModal: false,

        form: {
          id: null,
          name: "",
          contactEmail: "",
          phone: ""
        }
      };
    },

    methods: {
      async fetchData() {
        const res = await axios.get("/api/vendors", {
          params: { search: this.search, page: this.page }
        });

        this.vendors = res.data.data;
      },

      openModal(v = null) {
        this.showModal = true;

        if (v) {
          this.form = { ...v };
        } else {
          this.form = { id: null, name: "", contactEmail: "", phone: "" };
        }
      },

      async save() {
        if (this.form.id) {
          await axios.put("/api/vendors", this.form);
        } else {
          await axios.post("/api/vendors", this.form);
        }

        this.showModal = false;
        this.fetchData();
      },

      async remove(id) {
        if (!confirm("Delete this vendor?")) return;

        await axios.delete(`/api/vendors/${id}`);
        this.fetchData();
      },

      next() {
        this.page++;
        this.fetchData();
      },

      prev() {
        if (this.page > 1) {
          this.page--;
          this.fetchData();
        }
      }
    },

    mounted() {
      this.fetchData();
    }
  };
</script>

<style scoped>
  .vendor-container {
    padding: 30px;
    max-width: 1100px;
    margin: 0 auto;
    font-family: 'Inter', sans-serif;
    background-color: #f9fafb;
    min-height: 100vh;
  }

  /* Header */
  .header-section {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 25px;
  }

    .header-section h2 {
      color: #111827;
      font-size: 24px;
      font-weight: 700;
    }

  .controls {
    display: flex;
    gap: 12px;
  }

  .search-box input {
    padding: 10px 16px;
    border: 1px solid #d1d5db;
    border-radius: 8px;
    width: 280px;
    outline: none;
    transition: all 0.2s;
  }

    .search-box input:focus {
      border-color: #3b82f6;
      box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
    }

  /* Table Design */
  .table-card {
    background: white;
    border-radius: 12px;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
    overflow: hidden;
  }

  .styled-table {
    width: 100%;
    border-collapse: collapse;
    text-align: left;
  }

    .styled-table th {
      background-color: #f8fafc;
      padding: 14px 20px;
      color: #64748b;
      font-weight: 600;
      font-size: 13px;
      text-transform: uppercase;
      border-bottom: 1px solid #edf2f7;
    }

    .styled-table td {
      padding: 16px 20px;
      border-bottom: 1px solid #f1f5f9;
      color: #334155;
      font-size: 14px;
    }

  .vendor-name {
    font-weight: 600;
    color: #1e293b;
  }

  .styled-table tr:last-child td {
    border-bottom: none;
  }

  .styled-table tr:hover {
    background-color: #f8fafc;
  }

  /* Buttons */
  .btn {
    padding: 10px 20px;
    border-radius: 8px;
    font-weight: 500;
    cursor: pointer;
    border: none;
    transition: all 0.2s;
  }

  .btn-primary {
    background: #3b82f6;
    color: white;
  }

    .btn-primary:hover {
      background: #2563eb;
    }

  .btn-secondary {
    background: #e2e8f0;
    color: #475569;
  }

    .btn-secondary:hover {
      background: #cbd5e1;
    }

  .btn-icon {
    padding: 6px 12px;
    border-radius: 6px;
    margin: 0 4px;
    border: 1px solid #e2e8f0;
    background: white;
    cursor: pointer;
    font-size: 12px;
    transition: all 0.2s;
  }

    .btn-icon.edit:hover {
      color: #3b82f6;
      border-color: #3b82f6;
    }

    .btn-icon.delete:hover {
      color: #ef4444;
      border-color: #ef4444;
    }

  /* Modal */
  .modal-overlay {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: rgba(0, 0, 0, 0.4);
    display: flex;
    justify-content: center;
    align-items: center;
    z-index: 1000;
    backdrop-filter: blur(2px);
  }

  .modal-content {
    background: white;
    width: 450px;
    border-radius: 12px;
    box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1);
    overflow: hidden;
  }

  .modal-header {
    padding: 20px;
    border-bottom: 1px solid #f1f5f9;
    display: flex;
    justify-content: space-between;
    align-items: center;
  }

  .close-btn {
    background: none;
    border: none;
    font-size: 24px;
    cursor: pointer;
    color: #94a3b8;
  }

  .modal-body {
    padding: 20px;
  }

  .form-group {
    margin-bottom: 16px;
  }

    .form-group label {
      display: block;
      font-size: 13px;
      font-weight: 600;
      margin-bottom: 6px;
      color: #475569;
    }

    .form-group input {
      width: 100%;
      padding: 10px;
      border: 1px solid #d1d5db;
      border-radius: 6px;
      box-sizing: border-box;
    }

  .modal-footer {
    padding: 16px 20px;
    background: #f8fafc;
    display: flex;
    justify-content: flex-end;
    gap: 10px;
  }

  /* Pagination */
  .pagination {
    margin-top: 20px;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 15px;
  }

  .btn-page {
    padding: 8px 16px;
    border-radius: 6px;
    border: 1px solid #d1d5db;
    background: white;
    cursor: pointer;
  }

    .btn-page:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }

  /* Transition */
  .fade-enter-active, .fade-leave-active {
    transition: opacity 0.3s;
  }

  .fade-enter-from, .fade-leave-to {
    opacity: 0;
  }
</style>
