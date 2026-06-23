import { useState } from 'react';
import type { FormEvent } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { AuthCard } from '@/components/ui/AuthCard';
import { Alert, FormField, PrimaryButton, TextInput } from '@/components/ui/FormField';
import { acceptInvitation } from '@/lib/authApi';
import { getApiErrorMessage } from '@/lib/errors';

export function AcceptInvitationPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const [email, setEmail] = useState(searchParams.get('email') ?? '');
  const [token, setToken] = useState(searchParams.get('token') ?? '');
  const [fullName, setFullName] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);
    setSuccess(null);
    setIsSubmitting(true);

    try {
      const result = await acceptInvitation({
        email: email.trim(),
        token: token.trim(),
        password,
        fullName: fullName.trim() || undefined,
      });

      setSuccess(`Invitation accepted for ${result.tenantSlug}. You can sign in now.`);
      setTimeout(() => {
        navigate('/login');
      }, 1200);
    } catch (submitError) {
      setError(getApiErrorMessage(submitError, 'Invitation acceptance failed.'));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <AuthCard
      title="Accept invitation"
      description="Create your account to join the tenant workspace."
      footer={
        <Link to="/login" className="text-sky-400 hover:text-sky-300">
          Back to sign in
        </Link>
      }
    >
      <form className="space-y-4" onSubmit={handleSubmit}>
        {error ? <Alert tone="error">{error}</Alert> : null}
        {success ? <Alert tone="success">{success}</Alert> : null}

        <FormField label="Email">
          <TextInput
            type="email"
            value={email}
            onChange={(event) => setEmail(event.target.value)}
            required
          />
        </FormField>

        <FormField label="Invitation token">
          <TextInput
            value={token}
            onChange={(event) => setToken(event.target.value)}
            required
          />
        </FormField>

        <FormField label="Full name" hint="Optional">
          <TextInput
            value={fullName}
            onChange={(event) => setFullName(event.target.value)}
            placeholder="Jane Doe"
          />
        </FormField>

        <FormField label="Password">
          <TextInput
            type="password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            required
            autoComplete="new-password"
          />
        </FormField>

        <PrimaryButton type="submit" disabled={isSubmitting}>
          {isSubmitting ? 'Joining workspace...' : 'Accept invitation'}
        </PrimaryButton>
      </form>
    </AuthCard>
  );
}
