import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  listTimetables,
  createTimetable,
  updateTimetable,
  deleteTimetable,
} from "@/api/timetable";
import type {
  CreateTimetableRequest,
  UpdateTimetableRequest,
} from "@/types/timetable";

export function useTimetables(ownerId: string) {
  return useQuery({
    queryKey: ["timetables", ownerId],
    queryFn: () => listTimetables({ ownerId }),
    enabled: ownerId.trim().length > 0,
  });
}

export function useCreateTimetable() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (body: CreateTimetableRequest) => createTimetable(body),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["timetables"] }),
  });
}

export function useUpdateTimetable() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, body }: { id: string; body: UpdateTimetableRequest }) =>
      updateTimetable(id, body),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["timetables"] }),
  });
}

export function useDeleteTimetable() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => deleteTimetable(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["timetables"] }),
  });
}
