import { useEffect, useRef, useCallback } from "react";
import { ScanBarcode } from "lucide-react";

interface BarcodeScannerInputProps {
  onScan: (code: string) => void;
  disabled?: boolean;
}

const RAPID_THRESHOLD_MS = 80;

export default function BarcodeScannerInput({
  onScan,
  disabled,
}: BarcodeScannerInputProps) {
  const inputRef = useRef<HTMLInputElement>(null);
  const bufferRef = useRef("");
  const lastKeystrokeRef = useRef(0);

  const focusInput = useCallback(() => {
    if (!disabled) {
      inputRef.current?.focus();
    }
  }, [disabled]);

  const submitCode = useCallback(
    (code: string) => {
      const trimmed = code.trim();
      if (trimmed.length > 0) {
        onScan(trimmed);
      }
    },
    [onScan]
  );

  const handleInputKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === "Enter") {
      e.preventDefault();
      const value = inputRef.current?.value ?? "";
      submitCode(value);
      if (inputRef.current) {
        inputRef.current.value = "";
      }
      setTimeout(focusInput, 50);
    }
  };

  // Global keydown listener as fallback when input loses focus.
  // Handheld scanners type characters rapidly (<80ms between keystrokes)
  // then send Enter. We buffer rapid keystrokes and submit on Enter.
  useEffect(() => {
    if (disabled) return;

    const handleGlobalKeyDown = (e: KeyboardEvent) => {
      if (document.activeElement === inputRef.current) return;

      // Ignore modifier keys and non-printable keys (except Enter)
      if (e.ctrlKey || e.metaKey || e.altKey) return;

      const now = Date.now();

      if (e.key === "Enter") {
        if (bufferRef.current.length > 0) {
          e.preventDefault();
          submitCode(bufferRef.current);
          bufferRef.current = "";
        }
        return;
      }

      if (e.key.length === 1) {
        const elapsed = now - lastKeystrokeRef.current;
        if (elapsed > RAPID_THRESHOLD_MS * 3 && bufferRef.current.length > 0) {
          // Too slow gap -- likely human typing somewhere, reset buffer
          bufferRef.current = "";
        }
        bufferRef.current += e.key;
        lastKeystrokeRef.current = now;
      }
    };

    window.addEventListener("keydown", handleGlobalKeyDown);
    return () => window.removeEventListener("keydown", handleGlobalKeyDown);
  }, [disabled, submitCode]);

  // Auto-focus on mount and when re-enabled
  useEffect(() => {
    focusInput();
  }, [focusInput]);

  return (
    <div
      className="relative rounded-xl border-2 border-dashed border-muted-foreground/30 bg-muted/30 p-6 transition-colors focus-within:border-primary focus-within:bg-primary/5"
      onClick={focusInput}
    >
      <div className="flex flex-col items-center gap-3 text-center">
        <div className="flex h-16 w-16 items-center justify-center rounded-2xl bg-primary/10">
          <ScanBarcode className="h-8 w-8 text-primary" />
        </div>
        <div>
          <p className="font-semibold">Ready to Scan</p>
          <p className="mt-0.5 text-sm text-muted-foreground">
            Use the handheld barcode scanner on a menu item
          </p>
        </div>
        <input
          ref={inputRef}
          type="text"
          onKeyDown={handleInputKeyDown}
          onBlur={() => setTimeout(focusInput, 100)}
          disabled={disabled}
          className="sr-only"
          aria-label="Barcode scanner input"
          autoComplete="off"
          autoCorrect="off"
          autoCapitalize="off"
          spellCheck={false}
        />
        <div className="flex items-center gap-2 rounded-full bg-muted px-3 py-1.5 text-xs text-muted-foreground">
          <span className="relative flex h-2 w-2">
            <span className="absolute inline-flex h-full w-full animate-ping rounded-full bg-green-400 opacity-75" />
            <span className="relative inline-flex h-2 w-2 rounded-full bg-green-500" />
          </span>
          Scanner connected -- waiting for input
        </div>
      </div>
    </div>
  );
}
