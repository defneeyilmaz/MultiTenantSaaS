import { useCallback, useEffect, useState } from 'react';
import type { FormEvent } from 'react';
import { createProject, listProjects } from '@/lib/workspaceApi';
import { getApiErrorMessage } from '@/lib/errors';
import { hasPermission } from '@/lib/permissions';
import type { Project } from '@/types/workspace';
import { PageHeader } from '@/components/ui/PageHeader';
import { SectionCard } from '@/components/ui/SectionCard';
import { Alert, FormField, PrimaryButton, TextInput } from '@/components/ui/FormField';

export function ProjectsPage() {
  const [projects, setProjects] = useState<Project[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [isCreating, setIsCreating] = useState(false);

  const canView = hasPermission('projects.view');
  const canCreate = hasPermission('projects.create');

  const loadProjects = useCallback(async () => {
    if (!canView) {
      setLoading(false);
      return;
    }

    setLoading(true);
    setError(null);

    try {
      setProjects(await listProjects());
    } catch (loadError) {
      setError(getApiErrorMessage(loadError, 'Failed to load projects.'));
    } finally {
      setLoading(false);
    }
  }, [canView]);

  useEffect(() => {
    void loadProjects();
  }, [loadProjects]);

  async function handleCreate(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setIsCreating(true);
    setError(null);

    try {
      await createProject(name.trim(), description);
      setName('');
      setDescription('');
      await loadProjects();
    } catch (submitError) {
      setError(getApiErrorMessage(submitError, 'Failed to create project.'));
    } finally {
      setIsCreating(false);
    }
  }

  if (!canView) {
    return (
      <div>
        <PageHeader title="Projects" />
        <Alert tone="info">You need the projects.view permission to access this page.</Alert>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <PageHeader title="Projects" description="Tenant-scoped project list." />

      {error ? <Alert tone="error">{error}</Alert> : null}

      {canCreate ? (
        <SectionCard title="Create project">
          <form className="grid gap-4 md:grid-cols-2" onSubmit={handleCreate}>
            <FormField label="Name">
              <TextInput
                value={name}
                onChange={(event) => setName(event.target.value)}
                placeholder="Website redesign"
                required
              />
            </FormField>
            <FormField label="Description">
              <TextInput
                value={description}
                onChange={(event) => setDescription(event.target.value)}
                placeholder="Optional summary"
              />
            </FormField>
            <div className="md:col-span-2">
              <PrimaryButton type="submit" className="w-auto px-6" disabled={isCreating}>
                {isCreating ? 'Creating...' : 'Create project'}
              </PrimaryButton>
            </div>
          </form>
        </SectionCard>
      ) : null}

      <SectionCard title="All projects">
        {loading ? (
          <p className="text-sm text-slate-400">Loading projects...</p>
        ) : projects.length === 0 ? (
          <p className="text-sm text-slate-400">No projects yet.</p>
        ) : (
          <div className="space-y-3">
            {projects.map((project) => (
              <article
                key={project.id}
                className="rounded-xl border border-slate-800 bg-slate-950/50 px-4 py-3"
              >
                <h3 className="font-medium text-slate-100">{project.name}</h3>
                {project.description ? (
                  <p className="mt-1 text-sm text-slate-400">{project.description}</p>
                ) : null}
                <p className="mt-2 text-xs text-slate-600">
                  Created {new Date(project.createdAt).toLocaleString()}
                </p>
              </article>
            ))}
          </div>
        )}
      </SectionCard>
    </div>
  );
}
