export function AppAreaPage() {
  return (
    <section className="rounded-2xl border border-dashed border-slate-700 bg-slate-900/30 p-8">
      <p className="text-sm font-medium text-sky-400">Authenticated area</p>
      <h2 className="mt-2 text-2xl font-semibold">Tenant and user workspace</h2>
      <p className="mt-3 max-w-xl text-slate-400">
        Protected routes for tenant admin and end-user flows will plug into this shell in
        upcoming UI commits.
      </p>
    </section>
  );
}
