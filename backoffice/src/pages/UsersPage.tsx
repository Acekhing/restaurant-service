import { useState } from "react";
import {
  Plus,
  X,
  Users,
  Trash2,
  KeyRound,
  Pencil,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Select } from "@/components/ui/select";
import { Spinner } from "@/components/ui/spinner";
import { Badge } from "@/components/ui/badge";
import {
  useUsers,
  useCreateUser,
  useUpdateUser,
  useDeleteUser,
  useChangePassword,
} from "@/hooks/useUsers";
import { useRetailers } from "@/hooks/useRetailer";
import type { User } from "@/types/user";

function CreateUserForm({ onCreated }: { onCreated: () => void }) {
  const mutation = useCreateUser();
  const { data: retailers, isLoading: retailersLoading } = useRetailers();

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [fullName, setFullName] = useState("");
  const [role, setRole] = useState("waiter");
  const [retailerId, setRetailerId] = useState("");

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    mutation.mutate(
      {
        email: email.trim(),
        password,
        fullName: fullName.trim() || undefined,
        role,
        retailerId,
      },
      {
        onSuccess: () => {
          setEmail("");
          setPassword("");
          setFullName("");
          setRole("waiter");
          setRetailerId("");
          onCreated();
        },
      }
    );
  };

  return (
    <form
      onSubmit={handleSubmit}
      className="space-y-4 rounded-lg border bg-background p-6"
    >
      <h2 className="text-lg font-semibold">Add User</h2>
      <div className="grid gap-4 sm:grid-cols-2">
        <div>
          <label htmlFor="u-name" className="mb-1.5 block text-sm font-medium">
            Full Name
          </label>
          <Input
            id="u-name"
            value={fullName}
            onChange={(e) => setFullName(e.target.value)}
            placeholder="e.g. Kwame Mensah"
            required
          />
        </div>
        <div>
          <label htmlFor="u-email" className="mb-1.5 block text-sm font-medium">
            Email
          </label>
          <Input
            id="u-email"
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="user@example.com"
            required
          />
        </div>
        <div>
          <label htmlFor="u-password" className="mb-1.5 block text-sm font-medium">
            Password
          </label>
          <Input
            id="u-password"
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            placeholder="Min 6 characters"
            minLength={6}
            required
          />
        </div>
        <div>
          <label htmlFor="u-role" className="mb-1.5 block text-sm font-medium">
            Role
          </label>
          <Select
            id="u-role"
            value={role}
            onChange={(e) => setRole(e.target.value)}
          >
            <option value="waiter">Waiter</option>
            <option value="manager">Manager</option>
          </Select>
        </div>
        <div className="sm:col-span-2">
          <label htmlFor="u-retailer" className="mb-1.5 block text-sm font-medium">
            Retailer
          </label>
          <Select
            id="u-retailer"
            value={retailerId}
            onChange={(e) => setRetailerId(e.target.value)}
            required
            disabled={retailersLoading}
          >
            <option value="">
              {retailersLoading ? "Loading..." : "Select a retailer"}
            </option>
            {retailers?.map((r) => (
              <option key={r.id} value={r.id}>
                {r.businessName || r.id} ({r.retailerType})
              </option>
            ))}
          </Select>
        </div>
      </div>

      {mutation.isError && (
        <p className="text-sm text-destructive">
          {(mutation.error as any)?.response?.data?.error ||
            "Failed to create user. Please try again."}
        </p>
      )}

      <div className="flex justify-end gap-2">
        <Button type="submit" disabled={mutation.isPending}>
          {mutation.isPending && <Spinner className="h-4 w-4" />}
          Create User
        </Button>
      </div>
    </form>
  );
}

function EditUserDialog({
  user,
  onClose,
}: {
  user: User;
  onClose: () => void;
}) {
  const updateMutation = useUpdateUser();
  const passwordMutation = useChangePassword();
  const { data: retailers, isLoading: retailersLoading } = useRetailers();

  const [fullName, setFullName] = useState(user.fullName ?? "");
  const [role, setRole] = useState(user.role ?? "waiter");
  const [retailerId, setRetailerId] = useState(user.retailerId);
  const [isActive, setIsActive] = useState(user.isActive);
  const [newPassword, setNewPassword] = useState("");

  const handleUpdate = (e: React.FormEvent) => {
    e.preventDefault();
    updateMutation.mutate(
      {
        id: user.id,
        body: {
          fullName: fullName.trim() || undefined,
          role,
          retailerId,
          isActive,
        },
      },
      { onSuccess: onClose }
    );
  };

  const handlePasswordChange = () => {
    if (newPassword.length < 6) return;
    passwordMutation.mutate(
      { id: user.id, body: { password: newPassword } },
      { onSuccess: () => setNewPassword("") }
    );
  };

  return (
    <div className="space-y-4 rounded-lg border bg-background p-6">
      <div className="flex items-center justify-between">
        <h2 className="text-lg font-semibold">Edit User</h2>
        <Button variant="ghost" size="icon" onClick={onClose}>
          <X className="h-4 w-4" />
        </Button>
      </div>

      <form onSubmit={handleUpdate} className="space-y-4">
        <div className="grid gap-4 sm:grid-cols-2">
          <div>
            <label className="mb-1.5 block text-sm font-medium">Full Name</label>
            <Input
              value={fullName}
              onChange={(e) => setFullName(e.target.value)}
            />
          </div>
          <div>
            <label className="mb-1.5 block text-sm font-medium">Role</label>
            <Select value={role} onChange={(e) => setRole(e.target.value)}>
              <option value="waiter">Waiter</option>
              <option value="manager">Manager</option>
            </Select>
          </div>
          <div>
            <label className="mb-1.5 block text-sm font-medium">Retailer</label>
            <Select
              value={retailerId}
              onChange={(e) => setRetailerId(e.target.value)}
              disabled={retailersLoading}
              required
            >
              <option value="">Select a retailer</option>
              {retailers?.map((r) => (
                <option key={r.id} value={r.id}>
                  {r.businessName || r.id} ({r.retailerType})
                </option>
              ))}
            </Select>
          </div>
          <div className="flex items-end">
            <label className="flex items-center gap-2 cursor-pointer pb-2">
              <input
                type="checkbox"
                checked={isActive}
                onChange={(e) => setIsActive(e.target.checked)}
                className="accent-primary h-4 w-4"
              />
              <span className="text-sm font-medium">Active</span>
            </label>
          </div>
        </div>

        {updateMutation.isError && (
          <p className="text-sm text-destructive">Failed to update user.</p>
        )}

        <div className="flex justify-end">
          <Button type="submit" disabled={updateMutation.isPending}>
            {updateMutation.isPending && <Spinner className="h-4 w-4" />}
            Save Changes
          </Button>
        </div>
      </form>

      <div className="border-t pt-4">
        <label className="mb-1.5 block text-sm font-medium">Reset Password</label>
        <div className="flex gap-2">
          <Input
            type="password"
            value={newPassword}
            onChange={(e) => setNewPassword(e.target.value)}
            placeholder="New password (min 6 chars)"
            minLength={6}
            className="flex-1"
          />
          <Button
            type="button"
            variant="outline"
            onClick={handlePasswordChange}
            disabled={newPassword.length < 6 || passwordMutation.isPending}
          >
            {passwordMutation.isPending ? (
              <Spinner className="h-4 w-4" />
            ) : (
              <KeyRound className="h-4 w-4" />
            )}
            Reset
          </Button>
        </div>
        {passwordMutation.isSuccess && (
          <p className="mt-1 text-sm text-green-600">Password updated.</p>
        )}
      </div>
    </div>
  );
}

function UserRow({
  user,
  onEdit,
}: {
  user: User;
  onEdit: (user: User) => void;
}) {
  const deleteMutation = useDeleteUser();

  const handleDelete = () => {
    if (!confirm(`Delete user ${user.email}?`)) return;
    deleteMutation.mutate(user.id);
  };

  return (
    <li className="flex items-center gap-3 border-b px-4 py-3 last:border-b-0 hover:bg-muted/50 transition-colors">
      <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-muted">
        <Users className="h-4 w-4 text-muted-foreground" />
      </div>
      <div className="min-w-0 flex-1">
        <p className="font-medium">{user.fullName || user.email}</p>
        <p className="text-xs text-muted-foreground">{user.email}</p>
      </div>
      <div className="hidden sm:block text-sm text-muted-foreground">
        {user.retailerName || user.retailerId}
      </div>
      <Badge
        className={
          user.role === "manager"
            ? "bg-purple-100 text-purple-800"
            : "bg-sky-100 text-sky-800"
        }
      >
        {user.role || "waiter"}
      </Badge>
      <Badge
        className={
          user.isActive
            ? "bg-emerald-100 text-emerald-800"
            : "bg-red-100 text-red-800"
        }
      >
        {user.isActive ? "Active" : "Inactive"}
      </Badge>
      <button
        type="button"
        onClick={() => onEdit(user)}
        className="shrink-0 rounded p-1 text-muted-foreground hover:bg-muted hover:text-foreground transition-colors"
        title="Edit user"
      >
        <Pencil className="h-4 w-4" />
      </button>
      <button
        type="button"
        onClick={handleDelete}
        className="shrink-0 rounded p-1 text-muted-foreground hover:bg-muted hover:text-destructive transition-colors"
        title="Delete user"
        disabled={deleteMutation.isPending}
      >
        <Trash2 className="h-4 w-4" />
      </button>
    </li>
  );
}

export default function UsersPage() {
  const [showCreate, setShowCreate] = useState(false);
  const [editingUser, setEditingUser] = useState<User | null>(null);
  const [filterRetailerId, setFilterRetailerId] = useState("");
  const { data: retailers } = useRetailers();
  const {
    data: users,
    isLoading,
    isError,
  } = useUsers(filterRetailerId || undefined);

  return (
    <div className="mx-auto max-w-4xl space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">Users</h1>
          <p className="text-sm text-muted-foreground">
            Manage staff accounts linked to retailers
          </p>
        </div>
        <Button onClick={() => { setShowCreate((s) => !s); setEditingUser(null); }}>
          {showCreate ? (
            <>
              <X className="h-4 w-4" />
              Cancel
            </>
          ) : (
            <>
              <Plus className="h-4 w-4" />
              Add User
            </>
          )}
        </Button>
      </div>

      {showCreate && (
        <CreateUserForm onCreated={() => setShowCreate(false)} />
      )}

      {editingUser && (
        <EditUserDialog
          user={editingUser}
          onClose={() => setEditingUser(null)}
        />
      )}

      <div className="flex items-center gap-3">
        <span className="text-sm text-muted-foreground">Filter:</span>
        <Select
          value={filterRetailerId}
          onChange={(e) => setFilterRetailerId(e.target.value)}
          className="w-60"
        >
          <option value="">All retailers</option>
          {retailers?.map((r) => (
            <option key={r.id} value={r.id}>
              {r.businessName || r.id}
            </option>
          ))}
        </Select>
        {users && (
          <span className="text-sm text-muted-foreground">
            {users.length} user{users.length !== 1 ? "s" : ""}
          </span>
        )}
      </div>

      {isLoading && (
        <div className="flex justify-center py-16">
          <Spinner />
        </div>
      )}

      {isError && (
        <p className="py-12 text-center text-sm text-muted-foreground">
          Failed to load users. Make sure the API is running.
        </p>
      )}

      {users && (
        <>
          {users.length === 0 && !showCreate && (
            <div className="flex flex-col items-center justify-center py-16 text-center">
              <Users className="mb-3 h-10 w-10 text-muted-foreground/50" />
              <p className="text-sm text-muted-foreground">
                No users yet. Add one to get started.
              </p>
            </div>
          )}

          {users.length > 0 && (
            <ul className="rounded-lg border bg-background">
              {users.map((u) => (
                <UserRow
                  key={u.id}
                  user={u}
                  onEdit={(user) => {
                    setEditingUser(user);
                    setShowCreate(false);
                  }}
                />
              ))}
            </ul>
          )}
        </>
      )}
    </div>
  );
}
