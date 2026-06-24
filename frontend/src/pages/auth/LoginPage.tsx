import { useState } from 'react';
import type { FormEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { AuthCard } from '@/components/ui/AuthCard';
import { Alert, FormField, PrimaryButton, TextInput } from '@/components/ui/FormField';
import { login } from '@/lib/authApi';
import { saveAuthTokens } from '@/lib/authStorage';
import { getApiErrorMessage } from '@/lib/errors';

export function LoginPage() {
  const navigate = useNavigate();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [tenantSlug, setTenantSlug] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);

    try {
      const tokens = await login(email.trim(), password, tenantSlug.trim());
      saveAuthTokens(tokens);
      navigate('/app/admin');
    } catch (submitError) {
      setError(getApiErrorMessage(submitError, 'Login failed.'));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <AuthCard
      title="Sign in"
      description="Use your tenant workspace credentials."
      footer={
        <div className="space-y-2 text-slate-400">
          <p>
            New company?{' '}
            <Link to="/signup" className="text-sky-400 hover:text-sky-300">
              Create a workspace
            </Link>
          </p>
          <p>
            <Link to="/forgot-password" className="text-sky-400 hover:text-sky-300">
              Forgot password?
            </Link>
          </p>
        </div>
      }
    >
      <form className="space-y-4" onSubmit={handleSubmit}>
        {error ? <Alert tone="error">{error}</Alert> : null}

        <FormField label="Tenant slug">
          <TextInput
            value={tenantSlug}
            onChange={(event) => setTenantSlug(event.target.value)}
            placeholder="acme"
            required
            autoComplete="organization"
          />
        </FormField>

        <FormField label="Email">
          <TextInput
            type="email"
            value={email}
            onChange={(event) => setEmail(event.target.value)}
            placeholder="you@company.com"
            required
            autoComplete="email"
          />
        </FormField>

        <FormField label="Password">
          <TextInput
            type="password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            required
            autoComplete="current-password"
          />
        </FormField>

        <PrimaryButton type="submit" disabled={isSubmitting}>
          {isSubmitting ? 'Signing in...' : 'Sign in'}
        </PrimaryButton>
      </form>
    </AuthCard>
  );
}
