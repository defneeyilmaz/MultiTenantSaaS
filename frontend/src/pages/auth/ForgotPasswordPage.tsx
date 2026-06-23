import { useState } from 'react';
import type { FormEvent } from 'react';
import { Link } from 'react-router-dom';
import { AuthCard } from '@/components/ui/AuthCard';
import { Alert, FormField, PrimaryButton, TextInput } from '@/components/ui/FormField';
import { forgotPassword } from '@/lib/authApi';
import { getApiErrorMessage } from '@/lib/errors';

export function ForgotPasswordPage() {
  const [email, setEmail] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);
    setSuccess(null);
    setIsSubmitting(true);

    try {
      await forgotPassword(email.trim());
      setSuccess('If the email exists, a reset link instructions were sent.');
    } catch (submitError) {
      setError(getApiErrorMessage(submitError, 'Request failed.'));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <AuthCard
      title="Forgot password"
      description="We will send reset instructions if the account exists."
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
            placeholder="you@company.com"
            required
            autoComplete="email"
          />
        </FormField>

        <PrimaryButton type="submit" disabled={isSubmitting}>
          {isSubmitting ? 'Sending...' : 'Send reset instructions'}
        </PrimaryButton>
      </form>
    </AuthCard>
  );
}
