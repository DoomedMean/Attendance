<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const authStore = useAuthStore()

const today = new Date()
today.setHours(0, 0, 0, 0)

const viewYear = ref(today.getFullYear())
const viewMonth = ref(today.getMonth()) // 0-indexed

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
const DAY_NAMES = ['Min', 'Sen', 'Sel', 'Rab', 'Kam', 'Jum', 'Sab']

interface CalendarDay {
  date: Date | null
  day: number | null
}

const calendarDays = computed<CalendarDay[]>(() => {
  const year = viewYear.value
  const month = viewMonth.value
  const firstDay = new Date(year, month, 1)
  const lastDay = new Date(year, month + 1, 0)
  const days: CalendarDay[] = []

  // leading empty cells
  for (let i = 0; i < firstDay.getDay(); i++) {
    days.push({ date: null, day: null })
  }
  for (let d = 1; d <= lastDay.getDate(); d++) {
    days.push({ date: new Date(year, month, d), day: d })
  }
  // trailing empty cells to complete the last row
  while (days.length % 7 !== 0) {
    days.push({ date: null, day: null })
  }
  return days
})

function isClickable(date: Date | null): boolean {
  if (!date) return false
  return date <= today
}

function isToday(date: Date | null): boolean {
  if (!date) return false
  return date.getTime() === today.getTime()
}

function formatDate(date: Date): string {
  const y = date.getFullYear()
  const m = String(date.getMonth() + 1).padStart(2, '0')
  const d = String(date.getDate()).padStart(2, '0')
  return `${y}-${m}-${d}`
}

function onDayClick(date: Date | null) {
  if (!isClickable(date)) return
  router.push({ name: 'attendance', params: { date: formatDate(date!) } })
}

function prevMonth() {
  if (viewMonth.value === 0) {
    viewMonth.value = 11
    viewYear.value--
  } else {
    viewMonth.value--
  }
}

function nextMonth() {
  if (viewMonth.value === 11) {
    viewMonth.value = 0
    viewYear.value++
  } else {
    viewMonth.value++
  }
}

function handleLogout() {
  authStore.logout()
  router.push({ name: 'login' })
}
</script>

<template>
  <div class="calendar-page">
    <header class="page-header">
      <div class="header-content">
        <div class="header-title">
          <span class="header-icon">📋</span>
          <div>
            <h1>Attendance System</h1>
            <p>{{ authStore.employeeName }}</p>
          </div>
        </div>
        <button class="btn-logout" @click="handleLogout">Keluar</button>
      </div>
    </header>

    <main class="calendar-container">
      <div class="calendar-card">
        <div class="calendar-nav">
          <button class="nav-btn" @click="prevMonth">&#8249;</button>
          <h2 class="month-title">{{ MONTH_NAMES[viewMonth] }} {{ viewYear }}</h2>
          <button class="nav-btn" @click="nextMonth">&#8250;</button>
        </div>

        <div class="calendar-grid">
          <div v-for="name in DAY_NAMES" :key="name" class="day-name">{{ name }}</div>

          <div
            v-for="(cell, idx) in calendarDays"
            :key="idx"
            class="day-cell"
            :class="{
              empty: !cell.date,
              clickable: isClickable(cell.date),
              today: isToday(cell.date),
              future: cell.date !== null && !isClickable(cell.date),
            }"
            @click="onDayClick(cell.date)"
          >
            <span v-if="cell.day">{{ cell.day }}</span>
          </div>
        </div>

        <div class="calendar-legend">
          <span class="legend-item"><span class="dot dot-active"></span> Dapat dipilih</span>
          <span class="legend-item"><span class="dot dot-today"></span> Hari ini</span>
          <span class="legend-item"><span class="dot dot-future"></span> Belum tersedia</span>
        </div>
      </div>
    </main>
  </div>
</template>

<style scoped>
.calendar-page {
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
  justify-content: space-between;
}

.header-title {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  color: #fff;
}

.header-icon {
  font-size: 1.75rem;
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

.btn-logout {
  background: rgba(255, 255, 255, 0.2);
  color: #fff;
  border: 1.5px solid rgba(255, 255, 255, 0.5);
  border-radius: 6px;
  padding: 0.4rem 0.85rem;
  font-size: 0.85rem;
  cursor: pointer;
  transition: background 0.2s;
}

.btn-logout:hover {
  background: rgba(255, 255, 255, 0.35);
}

.calendar-container {
  max-width: 540px;
  margin: 2rem auto;
  padding: 0 1rem;
}

.calendar-card {
  background: #fff;
  border-radius: 16px;
  padding: 1.5rem;
  box-shadow: 0 4px 24px rgba(0, 0, 0, 0.08);
}

.calendar-nav {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 1.25rem;
}

.nav-btn {
  background: #f3f4f6;
  border: none;
  border-radius: 8px;
  width: 36px;
  height: 36px;
  font-size: 1.4rem;
  cursor: pointer;
  color: #374151;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: background 0.2s;
}

.nav-btn:hover {
  background: #e5e7eb;
}

.month-title {
  font-size: 1.1rem;
  font-weight: 700;
  color: #1a1a2e;
  margin: 0;
}

.calendar-grid {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  gap: 4px;
}

.day-name {
  text-align: center;
  font-size: 0.75rem;
  font-weight: 600;
  color: #9ca3af;
  padding: 0.4rem 0;
  text-transform: uppercase;
}

.day-cell {
  aspect-ratio: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 8px;
  font-size: 0.9rem;
  font-weight: 500;
  color: #9ca3af;
  cursor: default;
  transition:
    background 0.15s,
    transform 0.1s;
}

.day-cell.empty {
  background: transparent;
}

.day-cell.future {
  color: #d1d5db;
}

.day-cell.clickable {
  color: #374151;
  cursor: pointer;
}

.day-cell.clickable:hover {
  background: #ede9fe;
  color: #667eea;
  transform: scale(1.08);
}

.day-cell.today {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: #fff !important;
  font-weight: 700;
  box-shadow: 0 2px 8px rgba(102, 126, 234, 0.4);
}

.day-cell.today:hover {
  transform: scale(1.08);
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}

.calendar-legend {
  display: flex;
  gap: 1.25rem;
  justify-content: center;
  margin-top: 1.25rem;
  padding-top: 1rem;
  border-top: 1px solid #f3f4f6;
  flex-wrap: wrap;
}

.legend-item {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  font-size: 0.78rem;
  color: #6b7280;
}

.dot {
  width: 10px;
  height: 10px;
  border-radius: 50%;
  display: inline-block;
}

.dot-active {
  background: #374151;
}

.dot-today {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}

.dot-future {
  background: #d1d5db;
}
</style>
