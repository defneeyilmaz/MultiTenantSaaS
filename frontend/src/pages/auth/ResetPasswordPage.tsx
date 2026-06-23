import { useState } from 'react';
import type { FormEvent } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { AuthCard } from '@/components/ui/AuthCard';
import { Alert, FormField, PrimaryButton, TextInput } from '@/components/ui/FormField';
import { resetPassword } from '@/lib/authApi';
import { getApiErrorMessage } from '@/lib/errors';

export function ResetPasswordPage() {
  const [searchParams] = useSearchParams();
  const [email, setEmail] = useState(searchParams.get('email') ?? '');
  const [token, setToken] = useState(searchParams.get('token') ?? '');
  const [newPassword, setNewPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);
    setSuccess(null);
    setIsSubmitting(true);

    try {
      await resetPassword({
        email: email.trim(),
        token: token.trim(),
        newPassword,
      });

      setSuccess('Password updated. You can sign in with your new password.');
    } catch (submitError) {
      setError(getApiErrorMessage(submitError, 'Password reset failed.'));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <AuthCard
      title="Reset password"
      description="Paste the reset token from your email."
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

        <FormField label="Reset token">
          <TextInput
            value={token}
            onChange={(event) => setToken(event.target.value)}
            required
          />
        </FormField>

        <FormField label="New password">
          <TextInput
            type="password"
            value={newPassword}
            onChange={(event) => setNewPassword(event.target.value)}
            required
            autoComplete="new-password"
          />
        </FormField>

        <PrimaryButton type="submit" disabled={isSubmitting}>
          {isSubmitting ? 'Updating...' : 'Update password'}
        </PrimaryButton>
      </form>
    </AuthCard>
  );
}
