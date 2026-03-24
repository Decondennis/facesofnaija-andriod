import React, { createContext, useContext, useEffect, useState } from 'react';
import { getStoredCredentials, logout as logoutService } from '../services/authService';

interface AuthState {
  userId: string | null;
  sessionId: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
}

interface AuthContextValue extends AuthState {
  signIn: (userId: string, sessionId: string) => void;
  signOut: () => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [state, setState] = useState<AuthState>({
    userId: null,
    sessionId: null,
    isAuthenticated: false,
    isLoading: true,
  });

  useEffect(() => {
    getStoredCredentials().then((credentials) => {
      if (credentials) {
        setState({
          userId: credentials.userId,
          sessionId: credentials.sessionId,
          isAuthenticated: true,
          isLoading: false,
        });
      } else {
        setState((prev) => ({ ...prev, isLoading: false }));
      }
    });
  }, []);

  const signIn = (userId: string, sessionId: string) => {
    setState({
      userId,
      sessionId,
      isAuthenticated: true,
      isLoading: false,
    });
  };

  const signOut = async () => {
    if (state.userId && state.sessionId) {
      await logoutService(state.userId, state.sessionId);
    }
    setState({
      userId: null,
      sessionId: null,
      isAuthenticated: false,
      isLoading: false,
    });
  };

  return (
    <AuthContext.Provider value={{ ...state, signIn, signOut }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider');
  }
  return context;
}
