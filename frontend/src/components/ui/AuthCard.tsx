import type { ReactNode } from 'react';

export function AuthCard({
  title,
  description,
  children,
  footer,
}: {
  title: string;
  description?: string;
  children: ReactNode;
  footer?: ReactNode;
}) {
  return (
    <div className="rounded-2xl border border-slate-800 bg-slate-900/70 p-6 shadow-xl shadow-slate-950/40">
      <div className="mb-6">
        <h2 className="text-xl font-semibold">{title}</h2>
        {description ? <p className="mt-2 text-sm text-slate-400">{description}</p> : null}
      </div>

      {children}

      {footer ? <div className="mt-6 border-t border-slate-800 pt-4 text-sm">{footer}</div> : null}
    </div>
  );
}
