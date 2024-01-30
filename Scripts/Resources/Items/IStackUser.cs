public interface IStackUser {

    enum Result {
        Continue,
        Cancel
    }

    public string[] forIDs();

    public int priority() {
        return 0;
    }

    public Result use(ItemStack stack);

}
