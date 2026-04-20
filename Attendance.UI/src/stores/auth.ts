import { ref, computed } from 'vue'
import { defineStore } from 'pinia'

export const useAuthStore = defineStore('auth', () => {
  const employeeId = ref<string>(localStorage.getItem('employeeId') ?? '')
  const employeeName = ref<string>(localStorage.getItem('employeeName') ?? '')
  const isAuthenticated = computed(() => !!employeeId.value)

  function login(id: string, name: string): void {
    employeeId.value = id
    employeeName.value = name
    localStorage.setItem('employeeId', id)
    localStorage.setItem('employeeName', name)
  }

  function logout() {
    employeeId.value = ''
    employeeName.value = ''
    localStorage.removeItem('employeeId')
    localStorage.removeItem('employeeName')
  }

  return { employeeId, employeeName, isAuthenticated, login, logout }
})
