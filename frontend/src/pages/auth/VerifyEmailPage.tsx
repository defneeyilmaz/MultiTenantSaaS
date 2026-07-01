import { useState } from 'react';
import type { FormEvent } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { AuthCard } from '@/components/ui/AuthCard';
import { Alert, FormField, PrimaryButton, TextInput } from '@/components/ui/FormField';
import { verifyEmail } from '@/lib/authApi';
import { getApiErrorMessage } from '@/lib/errors';

export function VerifyEmailPage() {
  const [searchParams] = useSearchParams();
  const [email, setEmail] = useState(searchParams.get('email') ?? '');
  const [token, setToken] = useState(searchParams.get('token') ?? '');
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);
    setSuccess(null);
    setIsSubmitting(true);

    try {
      await verifyEmail(email, token);
      setSuccess('Email verified. You can sign in now.');
    } catch (submitError) {
      setError(getApiErrorMessage(submitError, 'Email verification failed.'));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <AuthCard
      title="Verify email"
      description="Paste the verification token from your email or API logs in development."
      footer={
        <Link to="/login" className="text-sky-400 hover:text-sky-300">
          Back to sign in
        </Link>
      }
    >
      <form className="space-y-4" onSubmit={handleSubmit}>
        <Alert tone="info">
          In development, the API logs the verification token after signup.
        </Alert>

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

        <FormField label="Verification token">
          <TextInput
            value={token}
            onChange={(event) => setToken(event.target.value)}
            required
          />
        </FormField>

        <PrimaryButton type="submit" disabled={isSubmitting}>
          {isSubmitting ? 'Verifying...' : 'Verify email'}
        </PrimaryButton>
      </form>
    </AuthCard>
  );
}
