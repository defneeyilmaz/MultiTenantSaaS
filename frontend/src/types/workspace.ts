export interface Project {
  id: string;
  name: string;
  description: string | null;
  createdAt: string;
}

export interface TaskItem {
  id: string;
  projectId: string;
  title: string;
  description: string | null;
  status: number | string;
  assignedToUserId: string | null;
  createdAt: string;
}

export const TASK_STATUSES = [
  { value: 1, label: 'Todo' },
  { value: 2, label: 'In Progress' },
  { value: 3, label: 'Done' },
] as const;

export function normalizeTaskStatus(status: number | string): number {
  if (typeof status === 'number') {
    return status;
  }

  switch (status) {
    case 'Todo':
      return 1;
    case 'InProgress':
      return 2;
    case 'Done':
      return 3;
    default:
      return 1;
  }
}

export function taskStatusLabel(status: number | string): string {
  const normalized = normalizeTaskStatus(status);
  return TASK_STATUSES.find((item) => item.value === normalized)?.label ?? 'Unknown';
}
