<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import {
  getAttendanceDetail,
  tapInOut,
  claimOvertimeApi,
  type AttendanceDetail,
} from '@/services/api'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

const dateParam = route.params.date as string
const attendance = ref<AttendanceDetail | null>(null)
const loading = ref(true)
const actionLoading = ref(false)
const error = ref('')

const isToday = computed(() => {
  const today = new Date()
  const todayStr = `${today.getFullYear()}-${String(today.getMonth() + 1).padStart(2, '0')}-${String(today.getDate()).padStart(2, '0')}`
  return dateParam === todayStr
})

// Button enable logic — all require today's date
const canClockIn = computed(
  () => isToday.value && !loading.value && !actionLoading.value && !attendance.value?.clockInTime,
)
const canBreakStart = computed(
  () =>
    isToday.value &&
    !loading.value &&
    !actionLoading.value &&
    !!attendance.value?.clockInTime &&
    !attendance.value?.breakStartTime &&
    !attendance.value?.clockOutTime,
)
const canBreakEnd = computed(
  () =>
    isToday.value &&
    !loading.value &&
    !actionLoading.value &&
    !!attendance.value?.breakStartTime &&
    !attendance.value?.breakEndTime &&
    !attendance.value?.clockOutTime,
)
const canClockOut = computed(
  () =>
    isToday.value &&
    !loading.value &&
    !actionLoading.value &&
    !!attendance.value?.clockInTime &&
    !attendance.value?.clockOutTime,
)

const overtimeHours = computed(() => attendance.value?.overTime ?? 0)

const canClaimOvertime = computed(
  () =>
    !loading.value &&
    !actionLoading.value &&
    !!attendance.value?.clockOutTime &&
    overtimeHours.value >= 1 &&
    !attendance.value?.isOTClimed,
)

async function loadDetails() {
  loading.value = true
  error.value = ''
  try {
    attendance.value = await getAttendanceDetail(authStore.employeeId, dateParam)
  } catch (err) {
    error.value =
      err instanceof Error ? err.message : 'Gagal memuat data kehadiran. Pastikan server berjalan.'
  } finally {
    loading.value = false
  }
}

async function handleClaimOvertime() {
  actionLoading.value = true
  error.value = ''
  try {
    await claimOvertimeApi(authStore.employeeId, dateParam)
    await loadDetails()
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Gagal claim overtime. Silakan coba lagi.'
  } finally {
    actionLoading.value = false
  }
}

async function handleTap(action: string) {
  actionLoading.value = true
  error.value = ''
  try {
    await tapInOut(authStore.employeeId, action)
    await loadDetails()
  } catch (err) {
    error.value =
      err instanceof Error ? err.message : `Gagal melakukan ${action}. Silakan coba lagi.`
  } finally {
    actionLoading.value = false
  }
}

function formatTime(time: string | null | undefined): string {
  if (!time) return '--:--'
  return time.substring(0, 5)
}

function formatHours(value: number | null | undefined): string {
  if (value == null) return '0j 0m'
  const h = Math.floor(value)
  const m = Math.round((value - h) * 60)
  return `${h}j ${m}m`
}

function formatDisplayDate(dateStr: string): string {
  const [year, month, day] = dateStr.split('-')
  const MONTH_NAMES = [
    'Januari',
    'Februari',
    'Maret',
    'April',
    'Mei',
    'Juni',
    'Juli',
    'Agustus',
    'September',
    'Oktober',
    'November',
    'Desember',
  ]
  return `${parseInt(day || '0', 10)} ${MONTH_NAMES[parseInt(month! || '0', 10) - 1]} ${year}`
}

function goBack() {
  router.push({ name: 'calendar' })
}

onMounted(loadDetails)
</script>

<template>
  <div class="attendance-page">
    <header class="page-header">
      <div class="header-content">
        <button class="btn-back" @click="goBack">&#8592;</button>
        <div class="header-title">
          <h1>Detail Kehadiran</h1>
          <p>{{ formatDisplayDate(dateParam) }}</p>
        </div>
      </div>
    </header>

    <main class="attendance-container">
      <!-- Loading state -->
      <div v-if="loading" class="loading-card">
        <div class="spinner"></div>
        <p>Memuat data kehadiran...</p>
      </div>

      <template v-else>
        <!-- Error message -->
        <div v-if="error" class="alert alert-error">{{ error }}</div>

        <!-- Time summary card -->
        <div class="summary-card">
          <h2 class="card-title">Rekap Waktu</h2>
          <div class="time-grid">
            <div class="time-item" :class="{ filled: !!attendance?.clockInTime }">
              <span class="time-label">Clock In</span>
              <span class="time-value">{{ formatTime(attendance?.clockInTime) }}</span>
            </div>
            <div class="time-item" :class="{ filled: !!attendance?.breakStartTime }">
              <span class="time-label">Break Start</span>
              <span class="time-value">{{ formatTime(attendance?.breakStartTime) }}</span>
            </div>
            <div class="time-item" :class="{ filled: !!attendance?.breakEndTime }">
              <span class="time-label">Break End</span>
              <span class="time-value">{{ formatTime(attendance?.breakEndTime) }}</span>
            </div>
            <div class="time-item" :class="{ filled: !!attendance?.clockOutTime }">
              <span class="time-label">Clock Out</span>
              <span class="time-value">{{ formatTime(attendance?.clockOutTime) }}</span>
            </div>
          </div>

          <div class="work-summary">
            <div class="summary-row">
              <span>Total Kerja</span>
              <strong class="workhours-value">{{ formatHours(attendance?.workHours) }}</strong>
            </div>
            <div class="summary-row overtime-row">
              <span>Lembur</span>
              <span class="overtime-right">
                <strong>{{ formatHours(attendance?.overTime) }}</strong>
                <span v-if="attendance?.isOTClimed" class="ot-badge claimed">Diklaim</span>
                <span v-else-if="overtimeHours >= 1" class="ot-badge eligible">Bisa Diklaim</span>
              </span>
            </div>
          </div>

          <div v-if="attendance?.notes" class="notes">
            <span class="notes-label">Catatan:</span> {{ attendance.notes }}
          </div>
        </div>

        <!-- Action buttons card -->
        <div class="actions-card">
          <h2 class="card-title">Aksi Kehadiran</h2>
          <p class="actions-hint">Tombol akan aktif sesuai dengan urutan pencatatan kehadiran.</p>

          <div class="btn-grid">
            <button
              class="tap-btn btn-clockin"
              :disabled="!canClockIn"
              @click="handleTap('ClockIn')"
            >
              <span class="tap-btn-icon">🟢</span>
              <span class="tap-btn-label">Clock In</span>
              <span v-if="attendance?.clockInTime" class="tap-btn-time">
                {{ formatTime(attendance.clockInTime) }}
              </span>
            </button>

            <button
              class="tap-btn btn-breakstart"
              :disabled="!canBreakStart"
              @click="handleTap('BreakStart')"
            >
              <span class="tap-btn-icon">☕</span>
              <span class="tap-btn-label">Break Start</span>
              <span v-if="attendance?.breakStartTime" class="tap-btn-time">
                {{ formatTime(attendance.breakStartTime) }}
              </span>
            </button>

            <button
              class="tap-btn btn-breakend"
              :disabled="!canBreakEnd"
              @click="handleTap('BreakEnd')"
            >
              <span class="tap-btn-icon">🔄</span>
              <span class="tap-btn-label">Break End</span>
              <span v-if="attendance?.breakEndTime" class="tap-btn-time">
                {{ formatTime(attendance.breakEndTime) }}
              </span>
            </button>

            <button
              class="tap-btn btn-clockout"
              :disabled="!canClockOut"
              @click="handleTap('ClockOut')"
            >
              <span class="tap-btn-icon">🔴</span>
              <span class="tap-btn-label">Clock Out</span>
              <span v-if="attendance?.clockOutTime" class="tap-btn-time">
                {{ formatTime(attendance.clockOutTime) }}
              </span>
            </button>
          </div>

          <div v-if="attendance?.clockOutTime" class="overtime-claim-section">
            <button
              class="tap-btn btn-overtime"
              :disabled="!canClaimOvertime"
              @click="handleClaimOvertime"
            >
              <span class="tap-btn-icon">⏱️</span>
              <span class="tap-btn-label">Claim Overtime</span>
              <span v-if="attendance?.isOTClimed" class="tap-btn-time">Sudah Diklaim</span>
              <span v-else-if="overtimeHours < 1" class="tap-btn-time">Min. 1 jam</span>
            </button>
          </div>

          <div v-if="actionLoading" class="action-loading">
            <div class="spinner spinner-sm"></div>
            <span>Menyimpan...</span>
          </div>
        </div>
      </template>
    </main>
  </div>
</template>

<style scoped>
.attendance-page {
  min-height: 100vh;
  background: #f0f4ff;
}

.page-header {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  padding: 1rem 1.5rem;
  box-shadow: 0 2px 12px rgba(102, 126, 234, 0.3);
}

.header-content {
  max-width: 540px;
  margin: 0 auto;
  display: flex;
  align-items: center;
  gap: 1rem;
}

.btn-back {
  background: rgba(255, 255, 255, 0.2);
  color: #fff;
  border: 1.5px solid rgba(255, 255, 255, 0.5);
  border-radius: 8px;
  width: 38px;
  height: 38px;
  font-size: 1.25rem;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  transition: background 0.2s;
}

.btn-back:hover {
  background: rgba(255, 255, 255, 0.35);
}

.header-title {
  color: #fff;
}

.header-title h1 {
  margin: 0;
  font-size: 1.1rem;
  font-weight: 700;
  line-height: 1.2;
}

.header-title p {
  margin: 0;
  font-size: 0.8rem;
  opacity: 0.85;
}

.attendance-container {
  max-width: 540px;
  margin: 1.5rem auto;
  padding: 0 1rem 2rem;
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.loading-card {
  background: #fff;
  border-radius: 16px;
  padding: 3rem;
  text-align: center;
  box-shadow: 0 4px 24px rgba(0, 0, 0, 0.08);
  color: #6b7280;
}

.spinner {
  width: 36px;
  height: 36px;
  border: 3px solid #e5e7eb;
  border-top-color: #667eea;
  border-radius: 50%;
  animation: spin 0.7s linear infinite;
  margin: 0 auto 1rem;
}

.spinner-sm {
  width: 18px;
  height: 18px;
  border-width: 2px;
  margin: 0;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

.alert {
  border-radius: 10px;
  padding: 0.85rem 1rem;
  font-size: 0.88rem;
}

.alert-error {
  background: #fef2f2;
  color: #b91c1c;
  border-left: 4px solid #ef4444;
}

.summary-card,
.actions-card {
  background: #fff;
  border-radius: 16px;
  padding: 1.5rem;
  box-shadow: 0 4px 24px rgba(0, 0, 0, 0.08);
}

.card-title {
  font-size: 1rem;
  font-weight: 700;
  color: #1a1a2e;
  margin: 0 0 1.1rem;
}

.time-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 0.75rem;
}

.time-item {
  background: #f9fafb;
  border-radius: 10px;
  padding: 0.85rem 1rem;
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  border: 1.5px solid #f3f4f6;
  transition: border-color 0.2s;
}

.time-item.filled {
  border-color: #c4b5fd;
  background: #faf5ff;
}

.time-label {
  font-size: 0.75rem;
  color: #9ca3af;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.03em;
}

.time-value {
  font-size: 1.35rem;
  font-weight: 700;
  color: #374151;
  font-variant-numeric: tabular-nums;
}

.time-item.filled .time-value {
  color: #6d28d9;
}

.work-summary {
  margin-top: 1rem;
  padding-top: 1rem;
  border-top: 1px solid #f3f4f6;
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
}

.summary-row {
  display: flex;
  justify-content: space-between;
  font-size: 0.9rem;
  color: #374151;
}

.summary-row.overtime strong {
  color: #d97706;
}

.notes {
  margin-top: 0.75rem;
  font-size: 0.85rem;
  color: #6b7280;
  background: #f9fafb;
  border-radius: 8px;
  padding: 0.6rem 0.85rem;
}

.notes-label {
  font-weight: 600;
  color: #374151;
}

.actions-hint {
  font-size: 0.82rem;
  color: #9ca3af;
  margin: -0.5rem 0 1rem;
}

.btn-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 0.75rem;
}

.tap-btn {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.3rem;
  padding: 1rem 0.75rem;
  border: 2px solid;
  border-radius: 12px;
  cursor: pointer;
  transition:
    opacity 0.2s,
    transform 0.1s,
    box-shadow 0.2s;
  background: #fff;
}

.tap-btn:not(:disabled):hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.12);
}

.tap-btn:disabled {
  opacity: 0.4;
  cursor: not-allowed;
  transform: none;
}

.tap-btn-icon {
  font-size: 1.5rem;
}

.tap-btn-label {
  font-size: 0.85rem;
  font-weight: 600;
}

.tap-btn-time {
  font-size: 1rem;
  font-weight: 700;
  font-variant-numeric: tabular-nums;
}

.btn-clockin {
  border-color: #86efac;
  color: #166534;
}

.btn-clockin:not(:disabled) {
  background: #f0fdf4;
}

.btn-clockin .tap-btn-time {
  color: #15803d;
}

.btn-breakstart {
  border-color: #fdba74;
  color: #92400e;
}

.btn-breakstart:not(:disabled) {
  background: #fffbeb;
}

.btn-breakstart .tap-btn-time {
  color: #d97706;
}

.btn-breakend {
  border-color: #93c5fd;
  color: #1e3a8a;
}

.btn-breakend:not(:disabled) {
  background: #eff6ff;
}

.btn-breakend .tap-btn-time {
  color: #2563eb;
}

.btn-clockout {
  border-color: #fca5a5;
  color: #7f1d1d;
}

.btn-clockout:not(:disabled) {
  background: #fff5f5;
}

.btn-clockout .tap-btn-time {
  color: #dc2626;
}

.action-loading {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  margin-top: 1rem;
  color: #667eea;
  font-size: 0.875rem;
}

.workhours-value {
  color: #166534;
  font-size: 1rem;
}

.overtime-row {
  align-items: center;
}

.overtime-right {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.ot-badge {
  font-size: 0.7rem;
  font-weight: 700;
  padding: 0.15rem 0.5rem;
  border-radius: 999px;
  text-transform: uppercase;
  letter-spacing: 0.04em;
}

.ot-badge.claimed {
  background: #dcfce7;
  color: #166534;
}

.ot-badge.eligible {
  background: #fef9c3;
  color: #854d0e;
}

.overtime-claim-section {
  margin-top: 0.75rem;
  padding-top: 0.75rem;
  border-top: 1px solid #f3f4f6;
}

.btn-overtime {
  width: 100%;
  flex-direction: row;
  justify-content: center;
  gap: 0.5rem;
  padding: 0.85rem 1rem;
  border-color: #fbbf24;
  color: #78350f;
}

.btn-overtime:not(:disabled) {
  background: #fffbeb;
}

.btn-overtime .tap-btn-time {
  color: #b45309;
  font-size: 0.8rem;
}
</style>
