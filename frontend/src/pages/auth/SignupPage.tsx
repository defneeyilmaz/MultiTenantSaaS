import { useState } from 'react';
import type { FormEvent } from 'react';
import { Link } from 'react-router-dom';
import { AuthCard } from '@/components/ui/AuthCard';
import { Alert, FormField, PrimaryButton, TextInput } from '@/components/ui/FormField';
import { companySignup } from '@/lib/authApi';
import { getApiErrorMessage } from '@/lib/errors';

export function SignupPage() {
  const [companyName, setCompanyName] = useState('');
  const [companySlug, setCompanySlug] = useState('');
  const [adminFullName, setAdminFullName] = useState('');
  const [adminEmail, setAdminEmail] = useState('');
  const [adminPassword, setAdminPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);
    setSuccess(null);
    setIsSubmitting(true);

    try {
      const result = await companySignup({
        companyName: companyName.trim(),
        companySlug: companySlug.trim() || undefined,
        adminFullName: adminFullName.trim() || undefined,
        adminEmail: adminEmail.trim(),
        adminPassword,
      });

      setSuccess(
        `Workspace "${result.tenantSlug}" created. Verify your email before signing in.`,
      );
    } catch (submitError) {
      setError(getApiErrorMessage(submitError, 'Signup failed.'));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <AuthCard
      title="Create workspace"
      description="Register a new company tenant and admin account."
      footer={
        <p className="text-slate-400">
          Already have an account?{' '}
          <Link to="/login" className="text-sky-400 hover:text-sky-300">
            Sign in
          </Link>
        </p>
      }
    >
      <form className="space-y-4" onSubmit={handleSubmit}>
        {error ? <Alert tone="error">{error}</Alert> : null}
        {success ? <Alert tone="success">{success}</Alert> : null}

        <FormField label="Company name">
          <TextInput
            value={companyName}
            onChange={(event) => setCompanyName(event.target.value)}
            placeholder="Acme Corp"
            required
          />
        </FormField>

        <FormField label="Company slug" hint="Optional. Generated from company name if empty.">
          <TextInput
            value={companySlug}
            onChange={(event) => setCompanySlug(event.target.value)}
            placeholder="acme"
          />
        </FormField>

        <FormField label="Admin full name" hint="Optional">
          <TextInput
            value={adminFullName}
            onChange={(event) => setAdminFullName(event.target.value)}
            placeholder="Jane Doe"
          />
        </FormField>

        <FormField label="Admin email">
          <TextInput
            type="email"
            value={adminEmail}
            onChange={(event) => setAdminEmail(event.target.value)}
            placeholder="admin@acme.com"
            required
          />
        </FormField>

        <FormField label="Password" hint="At least 8 characters with upper, lower, and digit.">
          <TextInput
            type="password"
            value={adminPassword}
            onChange={(event) => setAdminPassword(event.target.value)}
            required
            autoComplete="new-password"
          />
        </FormField>

        <PrimaryButton type="submit" disabled={isSubmitting}>
          {isSubmitting ? 'Creating workspace...' : 'Create workspace'}
        </PrimaryButton>
      </form>
    </AuthCard>
  );
}
