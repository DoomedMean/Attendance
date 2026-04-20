const BASE_URL = 'https://localhost:7059'

interface LoginResponse {
  message: string
  employeeId: string
  employeeName: string
}

export async function loginApi(email: string, password: string): Promise<LoginResponse> {
  const url = `${BASE_URL}/api/Employee/Login`
  const response = await fetch(url, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password }),
  })
  if (response.status === 401) {
    const body = await response.json().catch(() => ({}))
    throw new Error(body.message ?? 'Email or password incorrect')
  }
  if (!response.ok) throw new Error(`HTTP ${response.status}`)
  return response.json()
}

export interface AttendanceDetail {
  employeeId: string
  date: string | null
  clockInTime: string | null
  clockOutTime: string | null
  breakStartTime: string | null
  breakEndTime: string | null
  workHours: number
  overTime: number
  notes: string | null
  isOTClimed: boolean
}

export async function getAttendanceDetail(
  employeeId: string,
  date: string,
): Promise<AttendanceDetail | null> {
  const url = `${BASE_URL}/api/attend/Detail?employeeId=${encodeURIComponent(employeeId)}&date=${encodeURIComponent(date)}`
  const response = await fetch(url)
  if (!response.ok) {
    const body = await response.json().catch(() => ({}))
    throw new Error(body.message ?? `HTTP ${response.status}`)
  }
  const text = await response.text()
  if (!text || text === 'null') return null
  return JSON.parse(text) as AttendanceDetail
}

export async function tapInOut(employeeId: string, action: string): Promise<void> {
  const response = await fetch(`${BASE_URL}/api/attend/Tap`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ employeeId, action }),
  })
  if (!response.ok) {
    const body = await response.json().catch(() => ({}))
    throw new Error(body.message ?? `HTTP ${response.status}`)
  }
}

export async function claimOvertimeApi(employeeId: string, date: string): Promise<void> {
  const response = await fetch(`${BASE_URL}/api/attend/ClaimOvertime`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ employeeId, date }),
  })
  if (!response.ok) {
    const body = await response.json().catch(() => ({}))
    throw new Error(body.message ?? `HTTP ${response.status}`)
  }
}
