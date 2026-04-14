import { useState } from "react";
import { Plus, Clock, Trash2, X, ChevronRight, CalendarPlus } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Select } from "@/components/ui/select";
import { Spinner } from "@/components/ui/spinner";
import { cn } from "@/lib/utils";
import { useRetailerContext } from "@/context/RetailerContext";
import {
  useTimetables,
  useCreateTimetable,
  useDeleteTimetable,
} from "@/hooks/useTimetable";
import type { Timetable, TimetableOpening } from "@/types/timetable";

const DAYS_OF_WEEK = [
  "Monday",
  "Tuesday",
  "Wednesday",
  "Thursday",
  "Friday",
  "Saturday",
  "Sunday",
] as const;

function emptyOpening(day = ""): TimetableOpening {
  return { name: day, openingTime: "08:00", closingTime: "17:00" };
}

function CreateTimetableForm({
  ownerId,
  onDone,
}: {
  ownerId: string;
  onDone: () => void;
}) {
  const createMutation = useCreateTimetable();
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [openings, setOpenings] = useState<TimetableOpening[]>([
    emptyOpening("Monday"),
  ]);

  const usedDays = new Set(openings.map((o) => o.name));
  const availableDays = DAYS_OF_WEEK.filter((d) => !usedDays.has(d));

  const addOpening = () => {
    const nextDay = availableDays[0] ?? "";
    setOpenings((o) => [...o, emptyOpening(nextDay)]);
  };

  const addAllRemainingDays = () => {
    const newOpenings = availableDays.map((day) => emptyOpening(day));
    setOpenings((o) => [...o, ...newOpenings]);
  };

  const removeOpening = (idx: number) =>
    setOpenings((o) => o.filter((_, i) => i !== idx));

  const updateOpening = (
    idx: number,
    field: keyof TimetableOpening,
    value: string
  ) =>
    setOpenings((o) =>
      o.map((entry, i) => (i === idx ? { ...entry, [field]: value } : entry))
    );

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    const cleaned = openings.filter((o) => o.name.trim() !== "");
    if (cleaned.length === 0) return;

    createMutation.mutate(
      {
        name,
        description: description.trim() || undefined,
        ownerId,
        openings: cleaned,
      },
      {
        onSuccess: () => {
          setName("");
          setDescription("");
          setOpenings([emptyOpening("Monday")]);
          onDone();
        },
      }
    );
  };

  return (
    <form
      onSubmit={handleSubmit}
      className="space-y-5 rounded-lg border bg-background p-6"
    >
      <h2 className="text-lg font-semibold">Create Timetable</h2>

      <div className="grid gap-4 sm:grid-cols-2">
        <div>
          <label htmlFor="tt-name" className="mb-1.5 block text-sm font-medium">
            Name
          </label>
          <Input
            id="tt-name"
            value={name}
            onChange={(e) => setName(e.target.value)}
            placeholder="e.g. DayLight Sip"
            required
          />
        </div>

        <div>
          <label htmlFor="tt-desc" className="mb-1.5 block text-sm font-medium">
            Description
            <span className="ml-1 text-xs font-normal text-muted-foreground">
              (optional)
            </span>
          </label>
          <Input
            id="tt-desc"
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            placeholder="e.g. Standard weekday operating hours"
          />
        </div>
      </div>

      <div>
        <div className="mb-3 flex items-center justify-between">
          <label className="text-sm font-medium">
            Openings
            <span className="ml-1.5 text-xs font-normal text-muted-foreground">
              ({openings.length} day{openings.length !== 1 ? "s" : ""})
            </span>
          </label>
          <div className="flex gap-2">
            {availableDays.length > 1 && (
              <Button
                type="button"
                variant="ghost"
                size="sm"
                onClick={addAllRemainingDays}
              >
                <CalendarPlus className="h-3.5 w-3.5" />
                Add All Days
              </Button>
            )}
            <Button
              type="button"
              variant="outline"
              size="sm"
              onClick={addOpening}
              disabled={availableDays.length === 0}
            >
              <Plus className="h-3.5 w-3.5" />
              Add Day
            </Button>
          </div>
        </div>

        <div className="rounded-lg border">
          <div className="grid grid-cols-[1fr_120px_120px_40px] gap-3 border-b bg-muted/40 px-4 py-2 text-xs font-medium uppercase tracking-wide text-muted-foreground">
            <span>Day</span>
            <span>Opens</span>
            <span>Closes</span>
            <span />
          </div>

          <div className="divide-y">
            {openings.map((opening, idx) => (
              <div
                key={idx}
                className="grid grid-cols-[1fr_120px_120px_40px] items-center gap-3 px-4 py-2.5"
              >
                <Select
                  value={opening.name}
                  onChange={(e) => updateOpening(idx, "name", e.target.value)}
                  required
                >
                  <option value="" disabled>
                    Select day
                  </option>
                  {DAYS_OF_WEEK.map((day) => (
                    <option
                      key={day}
                      value={day}
                      disabled={usedDays.has(day) && opening.name !== day}
                    >
                      {day}
                    </option>
                  ))}
                </Select>
                <Input
                  type="time"
                  value={opening.openingTime}
                  onChange={(e) =>
                    updateOpening(idx, "openingTime", e.target.value)
                  }
                  required
                />
                <Input
                  type="time"
                  value={opening.closingTime}
                  onChange={(e) =>
                    updateOpening(idx, "closingTime", e.target.value)
                  }
                  required
                />
                <Button
                  type="button"
                  variant="ghost"
                  size="icon"
                  className="h-8 w-8"
                  onClick={() => removeOpening(idx)}
                  disabled={openings.length === 1}
                >
                  <Trash2 className="h-3.5 w-3.5 text-destructive" />
                </Button>
              </div>
            ))}
          </div>
        </div>

        {availableDays.length === 0 && (
          <p className="mt-2 text-xs text-muted-foreground">
            All days of the week have been added.
          </p>
        )}
      </div>

      {createMutation.isError && (
        <p className="text-sm text-destructive">
          Failed to create timetable. Please try again.
        </p>
      )}

      <div className="flex justify-end gap-2 pt-2">
        <Button type="button" variant="outline" onClick={onDone}>
          Cancel
        </Button>
        <Button type="submit" disabled={createMutation.isPending}>
          {createMutation.isPending && <Spinner className="h-4 w-4" />}
          Create Timetable
        </Button>
      </div>
    </form>
  );
}

function TimetableRow({
  timetable,
  onDelete,
}: {
  timetable: Timetable;
  onDelete: (id: string) => void;
}) {
  const [expanded, setExpanded] = useState(false);
  const sortedOpenings = [...timetable.openings].sort((a, b) => {
    const order = DAYS_OF_WEEK as readonly string[];
    return order.indexOf(a.name) - order.indexOf(b.name);
  });

  return (
    <li className="border-b last:border-b-0">
      <button
        type="button"
        onClick={() => setExpanded((o) => !o)}
        className="flex w-full items-center gap-3 px-4 py-3 text-left transition-colors cursor-pointer hover:bg-muted/40"
      >
        <ChevronRight
          className={cn(
            "h-4 w-4 shrink-0 text-muted-foreground transition-transform",
            expanded && "rotate-90"
          )}
        />
        <div className="min-w-0 flex-1">
          <span className="font-medium">{timetable.name}</span>
          {timetable.description && (
            <span className="ml-2 text-xs text-muted-foreground">
              {timetable.description}
            </span>
          )}
        </div>

        <span className="shrink-0 flex items-center gap-1 rounded-full bg-accent px-2 py-0.5 text-xs font-medium">
          {timetable.openings.length} day
          {timetable.openings.length !== 1 ? "s" : ""}
        </span>

        <span className="shrink-0 text-xs text-muted-foreground">
          {new Date(timetable.createdAt).toLocaleDateString()}
        </span>

        <button
          type="button"
          onClick={(e) => {
            e.stopPropagation();
            onDelete(timetable.id);
          }}
          className="shrink-0 rounded p-1 text-muted-foreground hover:bg-destructive/10 hover:text-destructive transition-colors"
          title="Delete timetable"
        >
          <Trash2 className="h-3.5 w-3.5" />
        </button>
      </button>

      {expanded && (
        <div className="border-t bg-muted/20 px-6 py-3 space-y-1.5">
          {sortedOpenings.map((o, i) => (
            <div
              key={i}
              className="grid grid-cols-[140px_1fr] items-center rounded-md bg-background px-3 py-2 text-sm"
            >
              <span className="font-medium">{o.name}</span>
              <span className="text-muted-foreground">
                {o.openingTime} &ndash; {o.closingTime}
              </span>
            </div>
          ))}
        </div>
      )}
    </li>
  );
}

export default function TimetableTab() {
  const { retailerId } = useRetailerContext();
  const [showCreate, setShowCreate] = useState(false);
  const { data: timetables, isLoading, isError } = useTimetables(retailerId);
  const deleteMutation = useDeleteTimetable();

  const handleDelete = (id: string) => {
    if (!confirm("Are you sure you want to delete this timetable?")) return;
    deleteMutation.mutate(id);
  };

  if (!retailerId) {
    return (
      <div className="flex flex-col items-center justify-center py-24 text-center">
        <Clock className="mb-4 h-12 w-12 text-muted-foreground/40" />
        <h2 className="text-lg font-semibold">Timetable</h2>
        <p className="mt-1 text-sm text-muted-foreground">
          Select a retailer to manage timetables.
        </p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-xl font-bold">Timetables</h2>
          <p className="text-sm text-muted-foreground">
            {timetables
              ? `${timetables.length} timetable${timetables.length !== 1 ? "s" : ""}`
              : ""}
          </p>
        </div>
        <Button
          onClick={() => setShowCreate((s) => !s)}
        >
          {showCreate ? (
            <>
              <X className="h-4 w-4" />
              Cancel
            </>
          ) : (
            <>
              <Plus className="h-4 w-4" />
              New Timetable
            </>
          )}
        </Button>
      </div>

      {showCreate && (
        <CreateTimetableForm
          ownerId={retailerId}
          onDone={() => setShowCreate(false)}
        />
      )}

      {isLoading && (
        <div className="flex justify-center py-16">
          <Spinner />
        </div>
      )}

      {isError && (
        <p className="py-12 text-center text-sm text-muted-foreground">
          Failed to load timetables. Make sure the API is running.
        </p>
      )}

      {timetables && (
        <>
          {timetables.length === 0 && !showCreate && (
            <div className="flex flex-col items-center justify-center py-16 text-center">
              <Clock className="mb-3 h-10 w-10 text-muted-foreground/50" />
              <p className="text-sm text-muted-foreground">
                No timetables yet. Create one to get started.
              </p>
            </div>
          )}

          {timetables.length > 0 && (
            <ul className="rounded-lg border bg-background">
              {timetables.map((t) => (
                <TimetableRow
                  key={t.id}
                  timetable={t}
                  onDelete={handleDelete}
                />
              ))}
            </ul>
          )}
        </>
      )}
    </div>
  );
}
