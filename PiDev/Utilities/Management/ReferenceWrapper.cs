/* Public Domain - 2025 Petar Petrov (PeterSvP)
 * https://pi-dev.com * https://store.steampowered.com/pub/pidev
 *
 * ============= Description =============
 * Lightweight wrapper class for value types to allow referencing and modifying structs by reference.
 * Supports implicit conversion to the underlying value type and overrides ToString() for convenience.
 *
 * ============= Usage =============
 * var myRef = new Ref<int>(5);
 * int value = myRef; // Implicitly converts to int
 * myRef.Value = 10;
 */

public sealed class Ref<T> where T : struct
{
    public Ref(T value) => Value = value;
    public T Value;
    public static implicit operator T(Ref<T> value) => value.Value;
    public override string ToString() => Value.ToString();
}