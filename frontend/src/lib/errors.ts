import axios from 'axios';

export function getApiErrorMessage(error: unknown, fallback = 'Something went wrong.'): string {
  if (axios.isAxiosError(error)) {
    const detail = error.response?.data?.detail;
    if (typeof detail === 'string' && detail.length > 0) {
      return detail;
    }

    const title = error.response?.data?.title;
    if (typeof title === 'string' && title.length > 0) {
      return title;
    }
  }

  return fallback;
}
