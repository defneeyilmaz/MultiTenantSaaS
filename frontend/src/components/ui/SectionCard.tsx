import type { ReactNode } from 'react';

export function SectionCard({
  title,
  description,
  children,
}: {
  title?: string;
  description?: string;
  children: ReactNode;
}) {
  return (
    <section className="rounded-2xl border border-slate-800 bg-slate-900/60 p-6">
      {title ? (
        <div className="mb-4">
          <h3 className="text-lg font-medium">{title}</h3>
          {description ? <p className="mt-1 text-sm text-slate-400">{description}</p> : null}
        </div>
      ) : null}
      {children}
    </section>
  );
}
