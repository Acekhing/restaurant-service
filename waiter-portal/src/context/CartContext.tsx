import {
  createContext,
  useContext,
  useReducer,
  type ReactNode,
  type Dispatch,
} from "react";

export interface CartItem {
  inventoryItemId: string;
  itemName: string;
  unitPrice: number;
  quantity: number;
  displayCurrency: string;
}

interface CartState {
  items: CartItem[];
  tableNumber: string;
  customerNotes: string;
  waiterName: string;
}

type CartAction =
  | { type: "ADD_ITEM"; payload: Omit<CartItem, "quantity"> }
  | { type: "REMOVE_ITEM"; payload: string }
  | { type: "SET_QUANTITY"; payload: { inventoryItemId: string; quantity: number } }
  | { type: "SET_TABLE"; payload: string }
  | { type: "SET_NOTES"; payload: string }
  | { type: "SET_WAITER"; payload: string }
  | { type: "CLEAR" };

function cartReducer(state: CartState, action: CartAction): CartState {
  switch (action.type) {
    case "ADD_ITEM": {
      const existing = state.items.find(
        (i) => i.inventoryItemId === action.payload.inventoryItemId
      );
      if (existing) {
        return {
          ...state,
          items: state.items.map((i) =>
            i.inventoryItemId === action.payload.inventoryItemId
              ? { ...i, quantity: i.quantity + 1 }
              : i
          ),
        };
      }
      return {
        ...state,
        items: [...state.items, { ...action.payload, quantity: 1 }],
      };
    }
    case "REMOVE_ITEM":
      return {
        ...state,
        items: state.items.filter(
          (i) => i.inventoryItemId !== action.payload
        ),
      };
    case "SET_QUANTITY": {
      if (action.payload.quantity <= 0) {
        return {
          ...state,
          items: state.items.filter(
            (i) => i.inventoryItemId !== action.payload.inventoryItemId
          ),
        };
      }
      return {
        ...state,
        items: state.items.map((i) =>
          i.inventoryItemId === action.payload.inventoryItemId
            ? { ...i, quantity: action.payload.quantity }
            : i
        ),
      };
    }
    case "SET_TABLE":
      return { ...state, tableNumber: action.payload };
    case "SET_NOTES":
      return { ...state, customerNotes: action.payload };
    case "SET_WAITER":
      return { ...state, waiterName: action.payload };
    case "CLEAR":
      return { ...state, items: [], tableNumber: "", customerNotes: "" };
    default:
      return state;
  }
}

const initialState: CartState = {
  items: [],
  tableNumber: "",
  customerNotes: "",
  waiterName: localStorage.getItem("waiter_name") ?? "",
};

interface CartContextValue {
  state: CartState;
  dispatch: Dispatch<CartAction>;
  totalAmount: number;
  totalItems: number;
}

const CartContext = createContext<CartContextValue>({
  state: initialState,
  dispatch: () => {},
  totalAmount: 0,
  totalItems: 0,
});

export function CartProvider({ children }: { children: ReactNode }) {
  const [state, dispatch] = useReducer(cartReducer, initialState);

  const totalAmount = state.items.reduce(
    (sum, i) => sum + i.unitPrice * i.quantity,
    0
  );
  const totalItems = state.items.reduce((sum, i) => sum + i.quantity, 0);

  return (
    <CartContext.Provider value={{ state, dispatch, totalAmount, totalItems }}>
      {children}
    </CartContext.Provider>
  );
}

export function useCart() {
  return useContext(CartContext);
}
