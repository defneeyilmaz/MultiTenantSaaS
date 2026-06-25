import { apiClient } from '@/lib/api';
import type { Project, TaskItem } from '@/types/workspace';

export async function listProjects(): Promise<Project[]> {
  const response = await apiClient.get<Project[]>('/api/projects');
  return response.data;
}

export async function createProject(name: string, description?: string): Promise<Project> {
  const response = await apiClient.post<Project>('/api/projects', {
    name,
    description: description?.trim() ? description.trim() : null,
  });
  return response.data;
}

export async function listTasks(projectId?: string): Promise<TaskItem[]> {
  const response = await apiClient.get<TaskItem[]>('/api/tasks', {
    params: projectId ? { projectId } : undefined,
  });
  return response.data;
}

export async function createTask(input: {
  projectId: string;
  title: string;
  description?: string;
  assignedToUserId?: string;
}): Promise<TaskItem> {
  const response = await apiClient.post<TaskItem>('/api/tasks', {
    projectId: input.projectId,
    title: input.title,
    description: input.description?.trim() ? input.description.trim() : null,
    assignedToUserId: input.assignedToUserId ?? null,
  });
  return response.data;
}

export async function updateTaskStatus(taskId: string, status: number): Promise<TaskItem> {
  const response = await apiClient.patch<TaskItem>(`/api/tasks/${taskId}/status`, { status });
  return response.data;
}
